// copyright discretelogics © 2011
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TeaTime.VSX;

#if false

namespace TeaTime.VSExt
{
    static class VsExtensions
    {
        #region keep for TeaBlend

        public static readonly Guid MiscellaneousFiles = new Guid("a2fe74e1-b743-11d0-ae1a-00a0c90fffc3");

        public static string GetProjectOutput(this Project p)
        {
            // TraceConfigProps(p);

            var properties = p.ConfigurationManager.ActiveConfiguration.Properties;
            string intermediatePath = (properties.Item("IntermediatePath")).Value.ToString();
            string outputPath = (properties.Item("OutputPath")).Value.ToString();

            OutputGroup built = GetBuilt(p);
            var urls = (object[])built.FileURLs;
            var surls = urls.Select(o => o.ToString());
            if (built.FileCount > 1)
            {
                TeaHousePackage.Instance.WriteMessage("More than one build output exists: " + surls.Joined(","));
            }
            var path = surls.First().Substring(8);
            path = path.Replace(intermediatePath, outputPath);
            return path;
        }

        static void TraceConfigProps(Project p)
        {
            foreach (Property o in p.ConfigurationManager.ActiveConfiguration.Properties)
            {
                Trace.WriteLine(o.Name + "=" + o.Value);
            }
        }

        static OutputGroup GetBuilt(Project p)
        {
            var built = p.ConfigurationManager.ActiveConfiguration.OutputGroups
                .OfType<OutputGroup>()
                .FirstOrDefault(og => og.CanonicalName == "Built");
            if (built == null) throw new Exception("TeaTime cannot determine Output of the current Project.");
            return built;
        }

        public static Project ToProject(this IVsHierarchy hierarchy)
        {
            if (hierarchy == null) throw new ArgumentNullException("hierarchy");
            object prjObject;

            if (hierarchy.GetProperty(0xfffffffe, (int)__VSHPROPID.VSHPROPID_ExtObject, out prjObject) >= 0)
            {
                return (Project)prjObject;
            }
            string name = "";
            try
            {
                hierarchy.GetCanonicalName(VSConstants.VSITEMID_ROOT, out name);
            }
            catch
            {
            }
            throw new Exception("Hierarchy=" + name);
        }

        public static Guid GetProjiectId(this IVsHierarchy h)
        {
            Guid guid;
            h.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out guid);
            return guid;
        }

        public static void TraceProperties(this Project p)
        {
            foreach (Property property in p.Properties)
            {
                try
                {
                    Trace.WriteLine("{0}={1}".Formatted(property.Name, property.Value));
                }
                catch (Exception)
                {
                    Trace.WriteLine("****");
                }
            }
        }

        public static IVsHierarchy ToHierarchy(this Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            IServiceProvider provider = new ServiceProvider(project.DTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            IVsSolution service = provider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (service == null)
            {
                throw new Exception("Cannot get SVsSolution service");
            }
            IVsHierarchy hierarchy;
            service.GetProjectOfUniqueName(project.UniqueName, out hierarchy);
            return hierarchy;
        }

        public static T GetService<T>(this IServiceProvider sp) where T : class
        {
            T service = (T)sp.GetService(typeof(T));
            if (service == null) throw new Exception("Could not get service " + typeof(T).FullName);
            return service;
        }

        #endregion

        public static IVsHierarchy GetCurrentHierarchy(this IServiceProvider provider)
        {
            DTE vs = (DTE)provider.GetService(typeof(DTE));
            if (vs == null) throw new InvalidOperationException("DTE not found.");

            return ToHierarchy(vs.SelectedItems.Item(1).ProjectItem.ContainingProject);
        }

        /// <summary>
        /// The project property IsDirty from MS does not work and is for internal use only 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool GetIsDirty(this Project p)
        {
            return p.AllItems().Any(pi => pi.IsDirty);
        }

        public static IEnumerable<ProjectItem> AllItems(this Project p)
        {
            return p.ProjectItems.OfType<ProjectItem>()
                .SelectMany(pi => pi.ProjectItems.OfType<ProjectItem>());
        }

        public static bool IsBuilt(this Project p)
        {
            var tBinary = File.GetLastWriteTime(p.GetProjectOutput());
            var physfs = "{" + VSConstants.GUID_ItemType_PhysicalFile.ToString().ToUpper() + "}";
            return p.AllItems()
                .Where(pi => pi.Kind == physfs)
                .All(pi => File.GetLastWriteTime(pi.FileNames[1]) <= tBinary);
        }

        public static Project ToDteProject(IVsProject project)
        {
            if (project == null) throw new ArgumentNullException("project");
            return (project as IVsHierarchy).ToProject();
        }


        public static ProjectItem ToProjectItem(IVsHierarchy hierarchy, uint itemid)
        {
            object o;
            int hr = hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out o);
            if (hr < 0)
            {
                throw new Exception("Project item is not available");
            }
            ProjectItem pi = o as ProjectItem;
            if (pi == null)
            {
                throw new Exception("Project item is not available");
            }
            return pi;
        }

        public static IVsProject3 ToVsProject(Project project)
        {
            if (project == null) throw new ArgumentNullException("project");
            IVsProject3 vsProject = ToHierarchy(project) as IVsProject3;
            if (vsProject == null)
            {
                throw new ArgumentException("Project is not a VS project.");
            }
            return vsProject;
        }

        public static IVsHierarchy GetStartupProject(this IVsSolutionBuildManager sbm)
        {
            IVsHierarchy h;
            sbm.get_StartupProject(out h);
            return h;
        }

        public static string GetCanonicalName(this IVsHierarchy h)
        {
            string name;
            h.GetCanonicalName((uint)VSConstants.VSITEMID.Root, out name);
            return name;
        }

        //public static Guid GetProjectGuid(IServiceProvider serviceProvider, Project p)
        //{			
        //    return GetProjectGuid(serviceProvider, ToHierarchy(p));
        //}

        //public static Guid GetProjectGuid(IServiceProvider serviceProvider, IVsHierarchy hierarchy)
        //{            
        //    Guid guid;			
        //    var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
        //    if (solution == null)
        //    {
        //        throw new InvalidOperationException("SolutionService is unavailable.");
        //    }
        //    solution.GetGuidOfProject(hierarchy, out guid);
        //    return guid;
        //}
    }
}

#endif