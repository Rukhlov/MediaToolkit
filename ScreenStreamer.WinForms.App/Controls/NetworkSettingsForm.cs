using MediaToolkit.Core;
using ScreenStreamer.Common;
using ScreenStreamer.WinForms.App;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestStreamer;
using TestStreamer.Controls;

namespace Test.Streamer.Controls
{
    public partial class NetworkSettingsForm : Form
    {
        public NetworkSettingsForm()
        {
            InitializeComponent();
            SetupRoutingSchema();
            LoadNetworks();
            LoadTransportItems();
        }

        private TransportMode transportMode = TransportMode.Tcp;

        private bool isMulticastMode = true;

        private StreamSession serverSettings = null;
        public void Init(StreamSession settings)
        {
            this.serverSettings = settings;

            var address = serverSettings.NetworkIpAddress;

            ComboBoxItem networkItem = null;
            if(address == "0.0.0.0")
            {
                networkItem = networkItems.FirstOrDefault(i => i.Tag == null);
            }
            else
            {
                networkItem = networkItems.FirstOrDefault(i => (i.Tag != null && ((IPAddressInformation)i.Tag).Address.ToString() == address));
            }


            if (networkItem != null)
            {
                networkComboBox.SelectedItem = networkItem;
            }
            

            textBoxStreamName.Text = serverSettings.StreamName;
            communicationPortNumeric.Value = serverSettings.CommunicationPort;
            multicastAddressTextBox.Text = serverSettings.MutlicastAddress;

            multicastPort1Numeric.Value = serverSettings.MutlicastPort1;
            multicastPort2Numeric.Value = serverSettings.MutlicastPort2;

            multicastRadioButton.Checked = serverSettings.IsMulticast;
            transportComboBox.Enabled = !serverSettings.IsMulticast;

        }

        private void videoSourceUpdateButton_Click(object sender, EventArgs e)
        {
            LoadNetworks();
        }


        private void multicastRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            isMulticastMode = serverSettings?.IsMulticast??false;
            SetupRoutingSchema();
        }

        private void unicastRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetupRoutingSchema();
        }

        private void LoadTransportItems()
        {
            var items = new List<TransportMode>
            {
                TransportMode.Tcp,
                //TransportMode.Udp,
            };

            transportComboBox.DataSource = items;
            transportComboBox.SelectedItem = transportMode;
        }

        private void LoadNetworks()
        {
            networkItems = GetNetworkItems();


            networkComboBox.DataSource = networkItems;
            networkComboBox.DisplayMember = "Name";
            
            var maxWidth = DropDownWidth(networkComboBox);
            networkComboBox.DropDownWidth = maxWidth;
        }


        private List<ComboBoxItem> networkItems = new List<ComboBoxItem>();

        private List<ComboBoxItem> GetNetworkItems()
        {
            var networks = new List<ComboBoxItem>();
            var networkAny = new ComboBoxItem
            {
                Name = "Any",
                Tag = null,
            };
            networks.Add(networkAny);

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                
                if (network.OperationalStatus == OperationalStatus.Up &&
                    network.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {

                    IPInterfaceProperties prop = network.GetIPProperties();

                    foreach (IPAddressInformation addr in prop.UnicastAddresses)
                    {
                        //var phAddr = network.GetPhysicalAddress();
                        //logger.Debug(addr.Address.ToString() + " (" + network.Name + " - " + network.Description + ") " + phAddr.ToString());

                        // IPv4
                        if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        networks.Add(new ComboBoxItem
                        {
                            Name = network.Name + " (" + addr.Address.ToString() + ")",
                            Tag = addr,
                        });

                    }
                }
            }

            return networks;
        }

        private int DropDownWidth(ComboBox comboBox)
        {
            int maxWidth = 0, temp = 0;
            foreach (var obj in comboBox.Items)
            {
                temp = TextRenderer.MeasureText(comboBox.GetItemText(obj), comboBox.Font).Width;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            return maxWidth;
        }

        public IPAddressInformation GetCurrentIpAddrInfo()
        {
            IPAddressInformation ipInfo = null;

            var obj = networkComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if (tag != null)
                    {
                        ipInfo = tag as IPAddressInformation;
                    }
                }
            }

            return ipInfo;
        }

        private void SetupRoutingSchema()
        {
            isMulticastMode = multicastRadioButton.Checked;

            multicastPanel.Enabled = isMulticastMode;

            transportComboBox.Enabled = !isMulticastMode;

            //multicastAddressTextBox.Enabled = isMulticastMode;

            //transportComboBox.Enabled = !isMulticastMode;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            var ipInfo = GetCurrentIpAddrInfo();         
            if (ipInfo != null)
            {
                serverSettings.NetworkIpAddress = ipInfo.Address.ToString();
            }

            transportMode = (TransportMode)transportComboBox.SelectedItem;
            if (isMulticastMode)
            {
                transportMode = TransportMode.Udp;
            }

            serverSettings.TransportMode = transportMode;

            serverSettings.CommunicationPort = (int)communicationPortNumeric.Value;

            serverSettings.IsMulticast = multicastRadioButton.Checked;
            serverSettings.MutlicastAddress = multicastAddressTextBox.Text;

            serverSettings.StreamName = this.textBoxStreamName.Text;
            serverSettings.MutlicastPort1 = (int)multicastPort1Numeric.Value;

            serverSettings.MutlicastPort2 = (int)multicastPort2Numeric.Value;

            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void findFreePortButton_Click(object sender, EventArgs e)
        {
            int currentPort = (int)communicationPortNumeric.Value;

            var freeTcpPorts = MediaToolkit.Utils.NetworkHelper.GetFreePortRange(ProtocolType.Tcp, 1, currentPort);
            if (freeTcpPorts != null)
            {
                var newPort = freeTcpPorts.FirstOrDefault();

                communicationPortNumeric.Value = (int)newPort;
            }
            else
            {
                MessageBox.Show("No avaliable tcp ports..");
            }
        }


    }
}
