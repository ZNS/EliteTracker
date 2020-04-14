using System.Net;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Server;
using ZNS.EliteTracker.Models.Indexes;

namespace ZNS.EliteTracker.Models
{
    public sealed class DB
    {
        private EmbeddableDocumentStore _Store;
        private static DB _Instance = null;
        private static readonly object _Padlock = new object();

        public EmbeddableDocumentStore Store
        {
            get { return _Store; }
        }

        DB()
        {
#if DEBUG
            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8081);
            _Store = new EmbeddableDocumentStore
            {
                DataDirectory = "App_Data/DB",
                RunInMemory = false,
                UseEmbeddedHttpServer = true,
                Configuration = {  Port = 8081 }
            };
#else
            _Store = new EmbeddableDocumentStore
            {
	            DataDirectory = "App_Data/DB",
                RunInMemory = false
            };
#endif
            _Store.Conventions.SaveEnumsAsIntegers = true;
            _Store.Initialize();
            
            //Create indexes
            new SolarSystem_Query().Execute(_Store);
            new Faction_Query().Execute(_Store);
            new EDDB_Query().Execute(_Store);
        }

        public IDocumentSession GetSession()
        {
            return _Store.OpenSession();
        }

        public IDatabaseCommands GetDatabaseCommands()
        {
            return _Store.DatabaseCommands.ForSystemDatabase();
        }

       public static DB Instance
        {
            get
            {
                lock (_Padlock)
                {
                    if (_Instance == null)
                    {
                        _Instance = new DB();
                    }
                    return _Instance;
                }
            }
        }
    }
}