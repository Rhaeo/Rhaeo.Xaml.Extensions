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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
      ////namespaceDeclaration.AddMembers(SyntaxFactory.XmlComment
      ////  (
      ////  SyntaxFactory.XmlElement(
      ////    SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("summary")),
      ////    SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("summary"))
      ////  )));

      var element =
        SyntaxFactory.XmlElement(
          SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("summary")),
          SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("summary"))
        ).WithLeadingTrivia(SyntaxFactory.DocumentationCommentExterior("/// "));

      var list = new SyntaxList<XmlNodeSyntax>().Add(element);

      var documentationTrivia = SyntaxFactory.Trivia(
        SyntaxFactory.DocumentationCommentTrivia(
          SyntaxKind.MultiLineDocumentationCommentTrivia,
          list
        )  
      );

      namespaceDeclaration = namespaceDeclaration.WithLeadingTrivia(documentationTrivia);

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