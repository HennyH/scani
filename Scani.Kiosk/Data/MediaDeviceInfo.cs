namespace Scani.Kiosk.Data
{
    public class MediaDeviceInfo
    {
        public MediaDeviceInfo(string deviceId, string groupId, string kind, string label)
        {
            this.DeviceId = deviceId;
            this.GroupId = groupId;
            this.Kind = kind;
            this.Label = label;
        }

        public string DeviceId { get; set; }
        public string GroupId { get; set; }
        public string Kind { get; set; }
        public string Label { get; set; }
    }
}
