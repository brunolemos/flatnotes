using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Windows.Storage;

namespace Keep.Utils.Migration
{
    public static class Migration
    {
        public const string STRUCTVERSION_KEY = "KeepSetting_StructureVersion";

        private static List<MigrationInterface> migrations = new List<MigrationInterface>() {
            new MigrationV1(),
            new MigrationV2(),
        };

        public static void Migrate(uint fromVersion) {
            Debug.WriteLine("Migration Started");
            for (int i = 0; i < migrations.Count; i++)
            {
                //MigrationInterface nextMigration = (i < migrations.Count - 1 ? migrations[i + 1] : null);
                //if (nextMigration == null) break;

                if (fromVersion < migrations[i].GetVersion())
                {
                    Debug.WriteLine("Migrating to v" + migrations[i].GetVersion().ToString() + "...");
                    migrations[i].Up();
                }
            }

        }
    }
}
