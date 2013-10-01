using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;


namespace InspectCodeHTML
{
    class HTMLCreator
    {

        private string Input;
        private string OutPut;
        private string template;
        private string issuetypeRowTemplate = @"<tr><td>{Name}</td><td>{Category}</td><td>{Description}</td><td><span style='color:{SeverityColor}'>'{Severity}</span></td></tr>";
        private string projectTemplate = "<h2>{Name}</h2> <input type=\"button\" style=\"cursor:pointer;\" onclick=\"toggle('{Name}')\" value=\"Show/Hide\" /><table><tr><th>Warnings</th><th>Error</th><th>Suggestion</th><th>Hint</th></tr><tr><td>{Warnings}</td><td>{ERROR}</td><td>{SUGGESTION}</td><td>{HINT}</td></table><div id=\"{Name}\" style=\"display:none;\">  <table border= \"1\"><tr><th>Type</th><th>Message</th><th>File</th><th>Line</th></tr>{CONTENT}</table></div>";
        private string issueRowTemplate = @"<tr style='color:{RowColor}'><td>{Type}</td><td>{Message}</td><td>{File}</td><td>{Line}</td>";
        private StringBuilder tmp = new StringBuilder();
        private Dictionary<string, IssueType> issuetypes;

        public HTMLCreator(string Input, string Output)
        {
            this.Input = Input;
            this.OutPut = Output;
            this.issuetypes = new Dictionary<string, IssueType>();
        }

        public void Run()
        {
            string currentProject="";
            bool projectActive=false;
            StringBuilder totalProjects = new StringBuilder();
            int errors = 0, warnings = 0, sug = 0, hint = 0;
            if (File.Exists(Input))
            {
                using (XmlReader reader = XmlReader.Create(Input))
                {
                    template = File.ReadAllText("ResultTemplate.html");  
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                {
                                    switch (reader.Name)
                                    {
                                        case "Report":
                                            {
                                                template = template.Replace("{PAGETITLE}", "Report from Version " + reader.GetAttribute("ToolsVersion"));
                                            }break;
                                        case "Solution":
                                            {
                                                template = template.Replace("{FILEPATH}", reader.ReadInnerXml());
                                            }break;
                                        case "Element":
                                            {
                                                template = template.Replace("{REPORTSCOPE}", reader.ReadInnerXml());
                                            }break;
                                        case "IssueType":
                                            {
                                                BuildIssueTypeObject(reader);
                                            }break;
                                        case "Issues":
                                            {
                                                BuildIssueTypeTable();
                                            }break;
                                        case "Project":
                                            {
                                                if( projectActive)
                                                {
                                                    currentProject = currentProject.Replace("{Warnings}", warnings.ToString()).Replace("{ERROR}", errors.ToString()).Replace("{SUGGESTION}", sug.ToString()).Replace("{HINT}", hint.ToString());
                                                    currentProject = currentProject.Replace("{CONTENT}", tmp.ToString());
                                                    totalProjects.Append(currentProject);
                                                    projectActive = false;
                                                }
                                                else
                                                {
                                                    errors = 0; warnings = 0; sug = 0; hint = 0;
                                                    projectActive = true;
                                                    tmp.Clear();
                                                    currentProject = this.projectTemplate.Replace("{Name}",reader.GetAttribute("Name"));
                                                }
                                            }break;
                                        case "Issue":
                                            {
                                                tmp.AppendLine( this.issueRowTemplate.Replace("{RowColor}",SeverityColor( this.issuetypes[ reader.GetAttribute("TypeId")].Severity)).Replace("{Type}",reader.GetAttribute("TypeId")).Replace("{Message}",reader.GetAttribute("Message")).Replace("{File}",reader.GetAttribute("File")).Replace("{Line}",reader.GetAttribute("Line")));
                                                switch(this.issuetypes[ reader.GetAttribute("TypeId")].Severity)
                                                {
                                                    case "WARNING":
                                                        {
                                                            warnings += 1;
                                                        }
                                                        break;
                                                    case "ERROR":
                                                        {
                                                            errors += 1;
                                                        }
                                                        break;
                                                    case "SUGGESTION":
                                                        {
                                                            sug += 1;
                                                        }
                                                        break;
                                                    case "HINT":
                                                        {
                                                            hint += 1;
                                                        }
                                                        break;
                                                }
                                            }break;
                                    }
                                }break;
                        }
                    }
                    if (projectActive)
                    {
                        currentProject = currentProject.Replace("{Warnings}", warnings.ToString()).Replace("{ERROR}", errors.ToString()).Replace("{SUGGESTION}", sug.ToString()).Replace("{HINT}", hint.ToString());
                        currentProject = currentProject.Replace("{CONTENT}", tmp.ToString());
                        totalProjects.Append(currentProject);
                        projectActive = false;
                    }
                    template = template.Replace("{PROJECTS}", totalProjects.ToString());
                    WriteFile();
                }
            }
            else
            {
                Print("Input file does not exists");
            }
        }
        

        private void BuildIssueTypeObject(XmlReader reader)
        {
            IssueType type = new IssueType() { ID = reader.GetAttribute("Id"), Category = reader.GetAttribute("Category"), Description = reader.GetAttribute("Description"), Severity = reader.GetAttribute("Severity") };
            this.issuetypes.Add(type.ID, type);
        }

        private void BuildIssueTypeTable()
        { 
            foreach( KeyValuePair<string,IssueType> p in this.issuetypes)
            {
                tmp.AppendLine(issuetypeRowTemplate.Replace("{Name}", p.Key).Replace("{Category}", p.Value.Category).Replace("{Description}", p.Value.Description).Replace("{Severity}", p.Value.Severity).Replace("{SeverityColor}",SeverityColor(p.Value.Severity)));
            }
            template = template.Replace("{IssueTypeDescirption}", tmp.ToString());
        }

        private string SeverityColor(string SeverityText)
        {
            switch (SeverityText)
            {
                case "WARNING":
                    {
                        return "#FF9732";
                    }
                    break;
                case "ERROR":
                    {
                        return "#FF0000";
                    }
                    break;
                case "SUGGESTION":
                    {
                        return "#80C0FF";
                    }
                    break;
                case "HINT":
                    {
                        return "#9AFF32";
                    }
                    break;
                default:
                    {
                        return "#000000";
                    }break;
            }
        }

        private void WriteFile()
        {
            File.WriteAllText(this.OutPut, this.template);
        }

        public void Print(string Message)
        {
            Console.WriteLine("INFO " + Message);
        }
    }
}
