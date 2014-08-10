using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
//using Microsoft.Phone.Info;
using Windows.Foundation;
using Firebase;

using Keep.Controllers;
using Keep.Models.Interfaces;

namespace Keep.Models
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

        [DataMember]
        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        private DateTime createdAt = DateTime.Now;

        [DataMember]
        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
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
