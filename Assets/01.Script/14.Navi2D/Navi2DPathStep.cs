using UnityEngine;
public enum Navi2DMoveType
{
    walk,
    AirMove
}


public class Navi2DPathStep
{
    public Navi2DNode fromNode;
    public Navi2DNode toNode;

    public Navi2DLinkData linkData;
    public Navi2DMoveType moveType;

    public Navi2DPathStep(Navi2DNode fromNode, Navi2DNode toNode, Navi2DMoveType moveType, Navi2DLinkData linkData = null)
    {
        this.fromNode = fromNode; 
        this.toNode = toNode;
        this.moveType = moveType;
        this.linkData = linkData;
    }
}
