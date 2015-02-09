using System.Net;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Document;
using Raven.Client.Embedded;
using ZNS.EliteTracker.Models.Indexes;

namespace ZNS.EliteTracker.Models
{
    public sealed class DB
    {
        private IDocumentStore _Store;
        private static DB _Instance = null;
        private static readonly object _Padlock = new object();

        public IDocumentStore Store
        {
            get { return _Store; }
        }

        private ICredentials _Credentials = null;

        DB()
        {            
#if DEBUG
            _Store = new EmbeddableDocumentStore
            {
                DataDirectory = "App_Data/DB",
                RunInMemory = false,
                UseEmbeddedHttpServer = true,
                Configuration = {  Port = 8080 }
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
        }

        public IDocumentSession GetSession()
        {
            if (_Credentials == null)
            {
                return _Store.OpenSession();
            }
            else
            {
                return _Store.OpenSession(new OpenSessionOptions() { Credentials = _Credentials });
            }
        }

        public IDatabaseCommands GetDatabaseCommands()
        {
            if (_Credentials == null)
            {
                return _Store.DatabaseCommands.ForSystemDatabase();
            }
            else
            {
                return _Store.DatabaseCommands.ForSystemDatabase().With(_Credentials);
            }
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