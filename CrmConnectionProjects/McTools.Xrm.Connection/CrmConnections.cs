﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace McTools.Xrm.Connection
{
    /// <summary>
    /// Stores the list of Crm connections
    /// </summary>
    public class CrmConnections
    {
        #region Variables

        private static readonly object _fileAccess = new object();

        #endregion Variables

        public CrmConnections(string name)
        {
            Connections = new List<ConnectionDetail>();
            Name = name;
        }

        public CrmConnections()
        {
        }

        #region Propriétés

        public bool ByPassProxyOnLocal { get; set; }

        /// <summary>
        /// Obtient ou définit la liste des connexions
        /// </summary>
        public List<ConnectionDetail> Connections { get; set; }

        /// <summary>
        /// Indicates if this connection list can be updated
        /// </summary>
        public bool IsReadOnly { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public string ProxyAddress { get; set; }

        public bool UseCustomProxy { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public bool UseInternetExplorerProxy { get; set; }

        public bool UseMruDisplay { get; set; }

        public string UserName { get; set; }
        public bool PublishAfterUpload { get; set; }
        public bool IgnoreExtensions { get; set; }
        public bool ExtendedLog { get; set; }

        #endregion Propriétés

        #region methods

        public static CrmConnections LoadFromFile(string filePath)
        {
            var crmConnections = new CrmConnections("Default");

            if (!Uri.IsWellFormedUriString(filePath, UriKind.Absolute) && !File.Exists(filePath))
            {
                return crmConnections;
            }

            using (var fStream = OpenStream(filePath))
            {
                return (CrmConnections)XmlSerializerHelper.Deserialize(fStream, typeof(CrmConnections), typeof(ConnectionDetail));
            }
        }

        public ConnectionDetail CloneConnection(ConnectionDetail detail)
        {
            var newDetail = (ConnectionDetail)detail.Clone();
            newDetail.ConnectionId = Guid.NewGuid();

            int cloneId = 0;
            string newName;
            do
            {
                cloneId++;
                newName = string.Format("{0} ({1})", newDetail.ConnectionName, cloneId);
            } while (Connections.Any(c => c.ConnectionName == newName));

            newDetail.ConnectionName = newName;

            Connections.Add(newDetail);

            return newDetail;
        }

        public void SerializeToFile(string filePath)
        {
            if (Uri.IsWellFormedUriString(filePath, UriKind.Absolute))
                return;

            lock (_fileAccess)
            {
                XmlSerializerHelper.SerializeToFile(this, filePath, typeof(ConnectionDetail));
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private static Stream OpenStream(string filePath)
        {
            if (Uri.IsWellFormedUriString(filePath, UriKind.Absolute))
            {
                var req = WebRequest.Create(filePath);
                req.Credentials = CredentialCache.DefaultCredentials;
                var resp = req.GetResponse();
                return resp.GetResponseStream();
            }

            return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        #endregion methods
    }
}