// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Rhaeo">
//   Licenced under the MIT licence.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Rhaeo.Xaml.Roslyn
{
  /// <summary>
  /// The program.
  /// </summary>
  public static class Program
  {
    #region Methods

    /// <summary>
    /// The main method.
    /// </summary>
    private static void Main()
    {
      var classDeclaration = SyntaxFactory.ClassDeclaration("Fatcow16x16Icons");

      var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("Rhaeo.Xaml.Extensions")).AddMembers(classDeclaration);

      var compilationUnit =
        SyntaxFactory.CompilationUnit()
          .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
          .AddMembers(namespaceDeclaration);

      using (var adhocWorkspace = new AdhocWorkspace())
      {
        var str = Formatter.Format(compilationUnit, adhocWorkspace);
        Console.WriteLine(str);
        Console.WriteLine();
        Console.WriteLine("Done");
      }

      Console.ReadLine();
    }

    #endregion
  }
}