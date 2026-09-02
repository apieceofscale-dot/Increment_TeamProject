using System;
using System.Collections.Generic;


public interface IBootStrapper
{

    int BootOrder { get; }

    void IBootStrapperInject(BootstrapContext context);

    void IBootStrapperInitialize();

    public interface IPoolable
    {
        void InitializePoolObj(Action returnAction);
        void OnSpawn();
        void OnDespawn();
    }


}

/// <summary>
/// 드랍 테이블 공급자, id 제공시 테이블 엔트리 제공
/// </summary>
public interface IDropTableSource
{

    bool TryGetEntries(int dropTableId, out IReadOnlyList<DropTableEntry> entries);
}