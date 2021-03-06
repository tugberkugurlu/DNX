// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.Framework.Runtime.Roslyn
{
    public class RoslynProjectMetadata
    {
        public RoslynProjectMetadata(CompilationContext context)
        {
            SourceFiles = context.Compilation
                                 .SyntaxTrees
                                 .Select(t => t.FilePath)
                                 .Where(p => !string.IsNullOrEmpty(p)) // REVIEW: Raw sources?
                                 .ToList();

            RawReferences = context.MetadataReferences.OfType<IMetadataEmbeddedReference>().Select(r =>
            {
                return new
                {
                    Name = r.Name,
                    Bytes = r.Contents
                };
            })
            .ToDictionary(a => a.Name, a => a.Bytes);

            References = context.MetadataReferences.OfType<IMetadataFileReference>()
                                            .Select(r => r.Path)
                                            .ToList();

            ProjectReferences = context.MetadataReferences.OfType<IMetadataProjectReference>()
                                                          .Select(r => r.ProjectPath)
                                                          .ToList();

            var formatter = new DiagnosticFormatter();

            var diagnostics = context.Compilation.GetDiagnostics()
                .Concat(context.Diagnostics)
                .ToList();

            Errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error || d.IsWarningAsError)
                                .Select(d => formatter.Format(d))
                                .ToList();

            Warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning)
                                  .Select(d => formatter.Format(d))
                                  .ToList();
        }

        public IList<string> SourceFiles
        {
            get;
            private set;
        }

        public IList<string> References
        {
            get;
            private set;
        }

        public IList<string> Errors
        {
            get;
            private set;
        }

        public IList<string> Warnings
        {
            get;
            private set;
        }

        public IDictionary<string, byte[]> RawReferences
        {
            get;
            private set;
        }

        public IList<string> ProjectReferences
        {
            get;
            private set;
        }
    }
}
