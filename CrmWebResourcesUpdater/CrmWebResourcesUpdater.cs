﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using CrmWebResourcesUpdater.Services.Helpers;
using CrmWebResourcesUpdater.Common;
using Microsoft.VisualStudio.Shell.Interop;
using System.Net;
using Task = System.Threading.Tasks.Task;
using System.Threading;
using CrmWebResourcesUpdater.Services;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// CrmWebResourceUpdater extension package class
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ProjectGuids.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class CrmWebResourcesUpdater : AsyncPackage
    {
        private DteInitializer dteInitializer;
        private ProjectHelper projectHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWebResources"/> class.
        /// </summary>
        public CrmWebResourcesUpdater()
        {
        }

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            await base.InitializeAsync(cancellationToken, progress);
            await Logger.InitializeAsync(this);

            SettingsService.Initialize(this);
            PublishService.Initialize(this);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await InitializeDTE(cancellationToken);
        }

        private async Task InitializeDTE(CancellationToken cancellationToken)
        {
            var dteObj = await this.GetServiceAsync(typeof(EnvDTE.DTE));
            var settings = await SettingsService.Instance.GetSettingsAsync();

            var extendedLog = false;
            if (settings!= null && settings.CrmConnections != null)
            {
                extendedLog = settings.CrmConnections.ExtendedLog;
            }

            if (dteObj == null) // The IDE is not yet fully initialized
            {
                Logger.WriteLine("Warning: DTE service is null. Seems that VisualStudio is not fully initialized.", extendedLog);
                //Logger.WriteLine("Waiting for DTE.", extendedLog);
                //var shellService = await this.GetServiceAsync(typeof(SVsShell)) as IVsShell;
                //this.dteInitializer = new DteInitializer(shellService, this.InitializeDTE);
            }
            else
            {
                try
                {
                    Logger.WriteLine("DTE service found.", extendedLog);
                    UpdateWebResources.Initialize(this);
                    UpdaterOptions.Initialize(this);
                    UpdateSelectedWebResources.Initialize(this);
                    CreateWebResource.Initialize(this);
                }
                catch (Exception ex)
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                }
            }
        }

        //private void InitializeDTE()
        //{
        //    IVsShell shellService;

        //    this.dte = GetService(typeof(SDTE)) as EnvDTE80.DTE2;
        //    var extendedLog = false;
        //    var settings = ProjectHelper.GetSettings();

        //    if (settings != null && settings.CrmConnections != null)
        //    {
        //        extendedLog = settings.CrmConnections.ExtendedLog;
        //    }

        //    if (this.dte == null) // The IDE is not yet fully initialized
        //    {
        //        Logger.WriteLine("Warning: DTE service is null. Seems that VisualStudio is not fully initialized.", extendedLog);
        //        Logger.WriteLine("Waiting for DTE.", extendedLog);
        //        shellService = this.GetService(typeof(SVsShell)) as IVsShell;
        //        this.dteInitializer = new DteInitializer(shellService, this.InitializeDTE);
        //    }
        //    else
        //    {
        //        Logger.WriteLine("DTE service found.", extendedLog);
        //        this.dteInitializer = null;
        //        UpdateWebResources.Initialize(this);
        //        UpdaterOptions.Initialize(this);
        //        UpdateSelectedWebResources.Initialize(this);
        //        CreateWebResource.Initialize(this);
        //    }
        //}

        #endregion
    }
}
