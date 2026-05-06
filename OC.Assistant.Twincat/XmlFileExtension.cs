using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat;

/// <summary>
/// <see cref="XmlFile"/> extension.
/// </summary>
internal static class XmlFileExtension
{
    extension(XmlFile xmlFile)
    {
        /// <summary>
        /// Gets the Twincat <see cref="XElement"/>.
        /// </summary>
        private XElement TcRoot => xmlFile.Root.GetOrCreateChild("Twincat");
        
        /// <summary>
        /// Gets the Settings <see cref="XElement"/>.
        /// </summary>
        private XElement Settings => xmlFile.TcRoot.GetOrCreateChild("Settings");
    
        /// <summary>
        /// Gets the HiL element.
        /// </summary>
        public XElement Hil => xmlFile.TcRoot.GetOrCreateChild("HiL");
    
        /// <summary>
        /// Gets the main program <see cref="XElement"/> or sets its content.
        /// </summary>
        public XElement Main
        {
            get => xmlFile.TcRoot.GetOrCreateChild("Main");
            set
            {
                xmlFile.TcRoot.GetOrCreateChild("Main").ReplaceNodes(value.Nodes());
                xmlFile.Save();       
            }
        }

        /// <summary>
        /// Gets or sets the PlcProjectName value.
        /// </summary>
        public string? PlcProjectName
        {
            get => xmlFile.Settings.GetOrCreateChild("PlcProjectName", "OC").Value;
            set => xmlFile.Settings.GetOrCreateChild("PlcProjectName").Value = value ?? "";
        }
    
        /// <summary>
        /// Gets or sets the PlcTaskName value.
        /// </summary>
        public string? PlcTaskName
        {
            get => xmlFile.Settings.GetOrCreateChild("PlcTaskName", "PlcTask").Value;
            set => xmlFile.Settings.GetOrCreateChild("PlcTaskName").Value = value ?? "";
        }
    }
}