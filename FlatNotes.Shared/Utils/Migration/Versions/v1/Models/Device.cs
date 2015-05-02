using FlatNotes.Utils.Migration.Versions.v1.Controllers;
using FlatNotes.Utils.Migration.Versions.v1.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Windows.Foundation;

namespace FlatNotes.Utils.Migration.Versions.v1.Models
{
    public class Devices : List<Device> { }

    public class Device : BaseModel, IIdentifiableModelInterface
    {
        public String GetID() { return DeviceUniqueId; }
        public DateTime GetCreatedAt() { return CreatedAt; }
        public DateTime GetUpdatedAt() { return UpdatedAt; }

        public string DeviceUniqueId { get { return DeviceController.DeviceID; } }
        public string DeviceManufacturer { get { return ""/*DeviceStatus.DeviceManufacturer*/; } }
        public Version OSVersion { get { return DeviceController.OSVersion; } }
        public string DeviceName { get { return ""/*DeviceStatus.DeviceName*/; } }
        public string DeviceFirmwareVersion { get { return ""/*DeviceStatus.DeviceFirmwareVersion*/; } }
        public string DeviceHardwareVersion { get { return ""/*DeviceStatus.DeviceHardwareVersion*/; } }
        public Size PhysicalScreenResolution { get { return DeviceController.PhysicalScreenResolution; } }

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        [DataMember(Name = "CreatedAt")]
        private DateTime createdAt = DateTime.Now;

        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
        [DataMember(Name = "UpdatedAt")]
        private DateTime updatedAt = DateTime.Now;

        public void Touch()
        {
            UpdatedAt = DateTime.Now;
        }

        public Device() { }

        public Device(DateTime createdAt)
        {
            CreatedAt = createdAt;
        }
    }
}
