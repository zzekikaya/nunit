﻿// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtainingn
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Engine.Internal;

namespace NUnit.Engine
{
    /// <summary>
    /// Wrapper class for the xml-formatted results produced
    /// by the test engine for most operations. The XML is
    /// stored as a string in order to allow serialization.
    /// </summary>
    [Serializable]
    public class TestEngineResult : ITestEngineResult
    {
        private List<string> xmlText = new List<string>();

        [NonSerialized]
        private List<XmlNode> xmlNodes = new List<XmlNode>();

        #region Static Methods

        /// <summary>
        /// Wrap a set of results in a single TestEngineResult. The result
        /// element is used only as a container to pass the results back to 
        /// the caller and no content aggregation is done.
        /// </summary>
        /// <param name="elementName">Name to be used for the wrapping element</param>
        /// <param name="results">The results to be wrapped.</param>
        /// <returns>A TestEngineResult wrapping the results.</returns>
        public static TestEngineResult Wrap(string elementName, IList<TestEngineResult> results)
        {
            var resultNodes = new List<XmlNode>();

            foreach (TestEngineResult result in results)
                foreach (XmlNode node in result.XmlNodes)
                    if (node.Name == "test-wrapper")
                        foreach (XmlNode child in node.ChildNodes)
                            resultNodes.Add(child);
                    else
                        resultNodes.Add(node);

            var wrapperNode = XmlHelper.CreateTopLevelElement(elementName);
            foreach (XmlNode node in resultNodes)
            {
                XmlNode import = wrapperNode.OwnerDocument.ImportNode(node, true);
                wrapperNode.AppendChild(import);
            }

            return new TestEngineResult(wrapperNode);
        }

        /// <summary>
        /// Aggregate a set of results in a single TestEngineResult. The result
        /// element name is specified by the first argument and values in 
        /// the subordinate results are aggregated in the final result.
        /// </summary>
        /// <param name="results">The results to be wrapped.</param>
        /// <returns>A TestEngineResult wrapping the results.</returns>
        public static TestEngineResult Aggregate(string elementName, TestPackage package, IList<TestEngineResult> results)
        {
            var resultNodes = new List<XmlNode>();
            foreach (var result in results)
                resultNodes.AddRange(result.XmlNodes);

            var combinedNode = Aggregate(elementName, package, resultNodes);

            return new TestEngineResult(combinedNode);
        }

        public static XmlNode Aggregate(string elementName, TestPackage package, IList<XmlNode> resultNodes)
        {
            XmlNode combinedNode = XmlHelper.CreateTopLevelElement(elementName);

            List<XmlNode> nodes = new List<XmlNode>();
            foreach (XmlNode node in resultNodes)
                if (node.Name == "test-wrapper")
                    foreach (XmlNode child in node.ChildNodes)
                        nodes.Add(child);
                else
                    nodes.Add(node);

            string status = "Inconclusive";
            double time = 0.0;
            int testcasecount = 0;
            int total = 0;
            int passed = 0;
            int failed = 0;
            int inconclusive = 0;
            int skipped = 0;
            int asserts = 0;

            foreach (var node in nodes)
            {
                switch (XmlHelper.GetAttribute(node, "result"))
                {
                    case "Skipped":
                        if (status == "Inconclusive")
                            status = "Skipped";
                        break;
                    case "Passed":
                        if (status != "Failed")
                            status = "Passed";
                        break;
                    case "Failed":
                        status = "Failed";
                        break;
                }

                testcasecount += XmlHelper.GetAttribute(node, "testcasecount", 0);
                total += XmlHelper.GetAttribute(node, "total", 0);
                time += XmlHelper.GetAttribute(node, "time", 0.0);
                passed += XmlHelper.GetAttribute(node, "passed", 0);
                failed += XmlHelper.GetAttribute(node, "failed", 0);
                inconclusive += XmlHelper.GetAttribute(node, "inconclusive", 0);
                skipped += XmlHelper.GetAttribute(node, "skipped", 0);
                asserts += XmlHelper.GetAttribute(node, "asserts", 0);

                XmlNode import = combinedNode.OwnerDocument.ImportNode(node, true);
                combinedNode.AppendChild(import);
            }

            XmlHelper.AddAttribute(combinedNode, "id", "2"); // TODO: Should not be hard-coded
            if (package.Name != null && package.Name != string.Empty)
                XmlHelper.AddAttribute(combinedNode, "name", package.Name);
            if (package.FullName != null && package.FullName != string.Empty)
                XmlHelper.AddAttribute(combinedNode, "fullname", package.FullName);
            XmlHelper.AddAttribute(combinedNode, "testcasecount", testcasecount.ToString());
            XmlHelper.AddAttribute(combinedNode, "result", status);
            XmlHelper.AddAttribute(combinedNode, "time", time.ToString());
            XmlHelper.AddAttribute(combinedNode, "total", total.ToString());
            XmlHelper.AddAttribute(combinedNode, "passed", passed.ToString());
            XmlHelper.AddAttribute(combinedNode, "failed", failed.ToString());
            XmlHelper.AddAttribute(combinedNode, "inconclusive", inconclusive.ToString());
            XmlHelper.AddAttribute(combinedNode, "skipped", skipped.ToString());
            XmlHelper.AddAttribute(combinedNode, "asserts", asserts.ToString());

            return combinedNode;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a TestResult from an XmlNode
        /// </summary>
        /// <param name="xml">An XmlNode representing the result</param>
        public TestEngineResult(XmlNode xml)
        {
            this.xmlNodes.Add(xml);
            this.xmlText.Add(xml.OuterXml);
        }

        /// <summary>
        /// Construct a test from a string holding xml
        /// </summary>
        /// <param name="xml">A string containing the xml result</param>
        public TestEngineResult(string xml)
        {
            this.xmlText.Add(xml);
        }

        /// <summary>
        /// Default constructor used when adding multiple results
        /// </summary>
        public TestEngineResult()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a flag indicating whether this is a single result
        /// having only one XmlNode associated with it.
        /// </summary>
        public bool IsSingle 
        {
            get { return xmlText.Count == 1; }
        }

        /// <summary>
        /// Gets a flag indicating whether result contains error nodes.
        /// </summary>
        public bool HasErrors
        {
            get 
            {
                foreach (XmlNode node in XmlNodes)
                    if (node.Name == "error") return true;
                return false; 
            }
        }

        public IList<TestEngineError> Errors
        {
            get 
            {
                var errors = new List<TestEngineError>();

                foreach (XmlNode errorNode in Xml.SelectNodes("error"))
                {
                    string message = XmlHelper.GetAttribute(errorNode, "message");
                    string stackTrace = XmlHelper.GetAttribute(errorNode, "stackTrace");
                    errors.Add(new TestEngineError(message, stackTrace));
                }

                return errors;
            }
        }

        /// <summary>
        /// Gets the xml representing a test result as an XmlNode
        /// </summary>
        public IList<XmlNode> XmlNodes
        {
            get
            {
                for (int i = xmlNodes.Count; i < xmlText.Count; i++)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlText[i]);
                    xmlNodes.Add(doc.FirstChild);
                }

                return xmlNodes;
            }
        }

        /// <summary>
        /// Gets the XML representing a single test result.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the result is empty or has multiple XML nodes.
        /// </exception>
        public XmlNode Xml
        {
            get 
            {
                if (!IsSingle)
                    throw new InvalidOperationException("May not use 'Xml' property on a result with multiple XmlNodes");
                    
                return XmlNodes[0]; 
            }
        }

        public void Add(string xml)
        {
            this.xmlText.Add(xml);
        }

        public void Add(XmlNode xml)
        {
            this.xmlText.Add(xml.OuterXml);
            this.xmlNodes.Add(xml);
        }

        #endregion
    }
}