using System;

namespace Keep.Models.Interfaces
{
    public interface IIdentifiableModelInterface
    {
        String GetID();
        DateTime GetCreatedAt();
        DateTime GetUpdatedAt();
        void Touch();
    }
}
