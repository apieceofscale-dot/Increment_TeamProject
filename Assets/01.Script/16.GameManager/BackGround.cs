using System.Collections.Generic;
using UnityEngine;

// [추가] 카메라 추적이 끝난 후 시차 이동과 반복 배치를 처리한다.
[DefaultExecutionOrder(100)]
public class BackGround : MonoBehaviour
{
    [SerializeField] GameObject parallax1;
    [SerializeField] GameObject parallax2;
    [SerializeField] GameObject parallax3;
    [SerializeField] GameObject parallax4;

    [Header("Horizontal Parallax")]
    [Tooltip("비워두면 MainCamera를 사용합니다.")]
    [SerializeField] Camera targetCamera;

    // [설명] 0은 지형처럼 고정, 1은 카메라와 같은 속도이다. 먼 배경일수록 크게 설정한다.
    [SerializeField, Range(0f, 1f)] float parallax1Ratio = 0.15f;
    [SerializeField, Range(0f, 1f)] float parallax2Ratio = 0.35f;
    [SerializeField, Range(0f, 1f)] float parallax3Ratio = 0.55f;
    [SerializeField, Range(0f, 1f)] float parallax4Ratio = 0.75f;

    // [변경] 하늘은 직렬화 필드 없이 기존 자식 배경에서 찾아 반복한다.
    private sealed class Layer
    {
        public SpriteRenderer source;
        public float originX;
        public readonly List<GameObject> copies = new List<GameObject>();
    }

    readonly List<Layer> layers = new List<Layer>();
    readonly List<GameObject> stationaryBackgrounds = new List<GameObject>();
    Camera trackedCamera;
    float initialCameraX;

    private void Awake()
    {
        // [수정] 네 시차 레이어 외의 기존 배경(하늘)도 반복 대상에 포함한다.
        // 복사본 생성 전에 한 번만 수집하여 재활성화 시 복사본을 원본으로 잡지 않는다.
        foreach (Transform child in transform)
        {
            GameObject candidate = child.gameObject;
            if (candidate == parallax1 || candidate == parallax2 ||
                candidate == parallax3 || candidate == parallax4) continue;
            if (candidate.GetComponent<SpriteRenderer>() != null)
                stationaryBackgrounds.Add(candidate);
        }
    }

    private void OnEnable()
    {
        layers.Clear();
        layers.Add(CreateLayer(parallax1));
        layers.Add(CreateLayer(parallax2));
        layers.Add(CreateLayer(parallax3));
        layers.Add(CreateLayer(parallax4));
        foreach (GameObject background in stationaryBackgrounds)
            layers.Add(CreateLayer(background));
        CaptureOrigin(ResolveCamera());
        // [수정] 처음 활성화되는 시점부터 카메라 양쪽을 채운다.
        UpdateBackground();
    }

    private Layer CreateLayer(GameObject layerObject)
    {
        if (layerObject == null) return null;
        SpriteRenderer sprite = layerObject.GetComponent<SpriteRenderer>();
        if (sprite == null || sprite.sprite == null)
        {
            Debug.LogWarning("[BackGround] 반복 레이어에 SpriteRenderer와 Sprite가 필요합니다.", layerObject);
            return null;
        }
        return new Layer { source = sprite, originX = layerObject.transform.position.x };
    }

    private Camera ResolveCamera() => targetCamera != null ? targetCamera : Camera.main;

    private void CaptureOrigin(Camera camera)
    {
        trackedCamera = camera;
        initialCameraX = camera != null ? camera.transform.position.x : 0f;
        foreach (Layer layer in layers)
            if (layer != null && layer.source != null)
                layer.originX = layer.source.transform.position.x;
    }

    private void LateUpdate()
    {
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        Camera camera = ResolveCamera();
        if (camera == null || !camera.isActiveAndEnabled)
        {
            trackedCamera = null;
            return;
        }
        if (trackedCamera != camera)
            CaptureOrigin(camera);

        float distanceX = camera.transform.position.x - initialCameraX;
        MoveLayer(layers[0], camera, distanceX, parallax1Ratio);
        MoveLayer(layers[1], camera, distanceX, parallax2Ratio);
        MoveLayer(layers[2], camera, distanceX, parallax3Ratio);
        MoveLayer(layers[3], camera, distanceX, parallax4Ratio);
        // [수정] 하늘의 기존 높이/배율과 위치 기준은 유지하고 좌우 빈 영역만 반복해서 채운다.
        for (int i = 4; i < layers.Count; i++)
            MoveLayer(layers[i], camera, distanceX, 0f);
    }

    private void MoveLayer(Layer layer, Camera camera, float distanceX, float ratio)
    {
        if (layer == null || layer.source == null) return;
        Transform source = layer.source.transform;
        Bounds bounds = layer.source.bounds;
        float width = bounds.size.x;
        if (width <= 0.0001f) return;

        // [추가] 피벗이 중앙이 아니어도 실제 렌더링 경계를 기준으로 이어 붙인다.
        float centerOffset = bounds.center.x - source.position.x;
        float baseX = layer.originX + distanceX * ratio;
        float depth = Vector3.Dot(bounds.center - camera.transform.position, camera.transform.forward);
        Vector3 left = camera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth));
        Vector3 right = camera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));
        float viewCenterX = (left.x + right.x) * 0.5f;

        // [추가] 카메라에 가까운 반복 구간으로 즉시 재배치하여 좌우 이동/순간이동을 처리한다.
        float tileOffset = Mathf.Round((viewCenterX - baseX - centerOffset) / width);
        Vector3 position = source.position;
        position.x = baseX + tileOffset * width;
        source.position = position;

        // [추가] 화면 폭보다 넓게 복사본을 확보한다. 확대/해상도 변경에도 빈 공간을 막는다.
        int sideCount = Mathf.CeilToInt(Mathf.Abs(right.x - left.x) * 0.5f / width) + 1;
        while (layer.copies.Count < sideCount * 2)
        {
            GameObject copy = Instantiate(source.gameObject, source.parent);
            copy.name = source.name + " (Parallax Repeat)";
            layer.copies.Add(copy);
        }

        for (int i = 0; i < layer.copies.Count; i++)
        {
            GameObject copy = layer.copies[i];
            bool visible = i < sideCount * 2 && source.gameObject.activeSelf;
            copy.SetActive(visible);
            if (!visible) continue;
            int tile = (i / 2 + 1) * (i % 2 == 0 ? -1 : 1);
            copy.transform.position = new Vector3(position.x + tile * width, position.y, position.z);
            copy.transform.rotation = source.rotation;
            copy.transform.localScale = source.localScale;
        }
    }

    private void OnDisable()
    {
        // [추가] 원본 배경은 유지하고, 이 스크립트가 만든 복사본만 정리한다.
        foreach (Layer layer in layers)
        {
            if (layer == null) continue;
            foreach (GameObject copy in layer.copies)
            {
                if (copy == null) continue;
                copy.SetActive(false);
                Destroy(copy);
            }
            layer.copies.Clear();
        }
        trackedCamera = null;
    }
}
