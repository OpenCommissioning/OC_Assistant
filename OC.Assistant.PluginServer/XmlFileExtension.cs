using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.PluginServer;

/// <summary>
/// <see cref="XmlFile"/> extension.
/// </summary>
internal static class XmlFileExtension
{
    extension(XmlFile xmlFile)
    {
        /// <summary>
        /// Gets the PluginServer <see cref="XElement"/>.
        /// </summary>
        private XElement PsRoot => xmlFile.Root.GetOrCreateChild("PluginServer");
        
        /// <summary>
        /// Gets the Settings <see cref="XElement"/>.
        /// </summary>
        private XElement Settings => xmlFile.PsRoot.GetOrCreateChild("Settings");

        /// <summary>
        /// Gets or sets the IpAddress value.
        /// </summary>
        public string IpAddress
        {
            get => xmlFile.Settings.GetOrCreateChild("IpAddress", TcpIpServer.DefaultIpAddress).Value;
            set => xmlFile.Settings.GetOrCreateChild("IpAddress").Value = value;
        }
    
        /// <summary>
        /// Gets or sets the Port value.
        /// </summary>
        public int Port
        {
            get => int
                .TryParse(xmlFile.Settings.GetOrCreateChild("Port", TcpIpServer.DefaultPort).Value, out var port) ? 
                port : TcpIpServer.DefaultPort;
            set => xmlFile.Settings.GetOrCreateChild("Port").Value = value.ToString();
        }
    }
}