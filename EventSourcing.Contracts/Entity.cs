using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Realms;

namespace EventSourcing.Contracts
{
    public interface IEntity : INotifyPropertyChanged
    {
        [IgnoreDataMember] IDataStore DataStore { get; set; }

        string PrimaryKey { get; set; }
        bool Attached { get; set; }

        void Attach(IDataStore dataStore)
        {
            Attached = true;
            DataStore = dataStore;
        }
    }

    // public abstract class Entity : RealmObject
    // {
    //     protected Entity()
    //     {
    //         PropertyChanged += (sender, args) =>
    //         {
    //             if (Attached) Save();
    //         };
    //     }
    //
    //     public void Save()
    //     {
    //         if (!Attached) throw new InvalidOperationException("Entity has not yet been attached, please call Attach first.");
    //         DataStore.Save(this);
    //     }
    //
    //     [Ignored]
    //     [IgnoreDataMember]
    //     [ContractRuntimeIgnored]
    //     private IDataStore DataStore { get; set; }
    //
    //     public abstract string PrimaryKey { get; }
    //     public bool Attached { get; private set; }
    //
    //     public void Attach(IDataStore dataStore)
    //     {
    //         Attached = true;
    //         DataStore = dataStore;
    //     }
    // }
}