using System;
using System.Activities.XamlIntegration;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Xaml;

namespace XamlToCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _validXaml;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            this._validXaml = true;
            this.statusInfo.Content = string.Empty;

            // Grab the text for the XAML source
            TextRange range = new TextRange(this.txtXAML.Document.ContentStart, this.txtXAML.Document.ContentEnd);

            if (!string.IsNullOrEmpty(range.Text))
            {
                // Generate and display the XAMl visual tree
                GenerateXAMLVisualTree(range.Text);

                // Only continue if the XAML is valid
                if (this._validXaml)
                {
                    XamlToCodeConverter cnv = new XamlToCodeConverter();

                    // Generate the code for this XAML
                    string srcCode = cnv.Convert(range.Text);

                    // Show user the code
                    FlowDocument mcFlowDoc = new FlowDocument();
                    Paragraph para = new Paragraph();
                    para.Inlines.Add(srcCode);
                    mcFlowDoc.Blocks.Add(para);
                    this.txtCode.Document = mcFlowDoc;

                    // Compile the code and show the visual tree for the code
                    CompilerResults res = cnv.CompileAssemblyFromLastCodeCompileUnit();
                    if (res.Errors.Count > 0)
                    {
                        foreach (CompilerError err in res.Errors)
                        {
                            string errorMsg = string.Format("Line: {0}, Column: {1}: {2}", err.Line, err.Column, err.ErrorText);
                            Debug.WriteLine(errorMsg);
                            this.statusInfo.Content = errorMsg;
                        }
                    }
                }
            }
        }

        private void GenerateXAMLVisualTree(string xaml)
        {
            using (MemoryStream ms = new MemoryStream(xaml.Length))
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    try
                    {
                        sw.Write(xaml);
                        sw.Flush();

                        ms.Seek(0, SeekOrigin.Begin);

                        // Load the Xaml
                        object content = ActivityXamlServices.Load(ms);
                    }
                    catch (XamlParseException x)
                    {
                        Debug.WriteLine("XAML Parse error: Line:{0}, Position:{1}, Error: {2}", new object[] { x.LineNumber, x.LinePosition, x.Message });
                        this.statusInfo.Content = x.Message;
                        this._validXaml = false;
                    }
                    catch (Exception ex)
                    {
                        // Generic message
                        this.statusInfo.Content = ex.Message;
                        this._validXaml = false;
                    }
                }
            }
        }
    }
}
