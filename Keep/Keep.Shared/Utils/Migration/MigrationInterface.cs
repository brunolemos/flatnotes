using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Utils.Migration
{
    interface MigrationInterface
    {
        void Up();
        void Down();
        uint GetVersion();
    }
}
