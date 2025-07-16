using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Helpers
{
    public class DeviceInfo
    {
        public string GetGeneratedSHA()
        {
            string info = GetMotherBoardID() + ProcessorId() + HddId() + GetMacAddress();

            return GetHashString(info);
        }

        public string GetMotherBoardID()
        {
          
            string mbInfo = String.Empty;
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (PropertyData propData in wmiClass.Properties)
            {
                //mbInfo += propData.Name + "\n";

                if (propData.Name == "SerialNumber")
                    mbInfo = Convert.ToString(propData.Value);
            }

            return mbInfo;
        }
       
        public string ProcessorId()
        {
            string info = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
    "select * from " + "Win32_Processor");

            foreach (ManagementObject share in searcher.Get())
            {
                // Some Codes ...
                foreach (PropertyData propData in share.Properties)
                {
                    //some codes ...
                    //info += propData.Name + "\n";

                    if (propData.Name == "ProcessorId")
                        info = Convert.ToString(propData.Value);//String.Format("{0,-25}{1}", propData.Name, Convert.ToString(propData.Value));
                }

            }
            return info;
        }

        public string HddId()
        {
            string info = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
    "select * from " + "Win32_DiskDrive");

            foreach (ManagementObject share in searcher.Get())
            {
                // Some Codes ...
                foreach (PropertyData propData in share.Properties)
                {
                    //some codes ...
                    //info += propData.Name + "\n";

                    if (propData.Name == "SerialNumber")
                        info = Convert.ToString(propData.Value);//String.Format("{0,-25}{1}", propData.Name, Convert.ToString(propData.Value));
                }

            }
            return info;
        }

        public string GetMacAddress()
        {
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddresses;
        }

        public string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

       
    }


}
