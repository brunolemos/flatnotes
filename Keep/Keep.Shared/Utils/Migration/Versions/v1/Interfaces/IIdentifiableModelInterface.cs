using System;

namespace Keep.Utils.Migration.Versions.v1.Interfaces
{
    public interface IIdentifiableModelInterface
    {
        String GetID();
        DateTime GetCreatedAt();
        DateTime GetUpdatedAt();
        void Touch();
    }
}
