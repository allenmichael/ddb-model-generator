using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GenerateDDBModel.Generator
{
    public static class Generator
    {
        public static IdentifierNameSyntax HashKeyToken = IdentifierName("DynamoDBHashKey");
        public static IdentifierNameSyntax RangeKeyToken = IdentifierName("DynamoDBRangeKey");
        public static IdentifierNameSyntax GlobalSecondaryIndexHashKeyToken = IdentifierName("DynamoDBRangeKey");
        public static UsingDirectiveSyntax DynamoUsingToken = UsingDirective(ParseName("Amazon.DynamoDBv2.DataModel"));
        public static string DefaultNumberType = "decimal";
        public static string DefaultBinaryType = "byte[]";
        public static string DefaultModelSuffix = "DDBModel";
        public static string HashType = "HASH";
        public static string RangeType = "RANGE";
        public static AttributeSyntax TableNameAttribute(string name)
        {
            return Attribute(IdentifierName("DynamoDBTable"))
                .WithArgumentList(
                    AttributeArgumentList(
                        SingletonSeparatedList<AttributeArgumentSyntax>(
                            AttributeArgument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal($"{name}")
                                )
                            )
                        )
                    )
                );
        }

        public static string FindType(string typeName)
        {
            switch (typeName)
            {
                case "S":
                    return "string";
                case "B":
                    return DefaultBinaryType;
                case "N":
                    return DefaultNumberType;
                default:
                    throw new InvalidDataException("Couldn't determine type of Dynamo attribute");
            }
        }

        public static string GetClassName(TableDescription ddbTable)
        {
            return $"{ddbTable.TableName}{DefaultModelSuffix}";
        }

        public static string FromDDBTableModel(TableDescription ddbTable)
        {
            var className = GetClassName(ddbTable);
            var code = CompilationUnit();
            code = code.AddUsings(DynamoUsingToken.NormalizeWhitespace());
            var classDeclaration = ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddAttributeLists(
                    AttributeList(
                        SingletonSeparatedList<AttributeSyntax>(
                            TableNameAttribute(ddbTable.TableName)
                        )
                    )
                );
            var properties = new List<MemberDeclarationSyntax>();
            foreach (var ks in ddbTable.KeySchema)
            {
                var attr = ddbTable.AttributeDefinitions.Single(ad => ad.AttributeName == ks.AttributeName);

                // if (prop == null || string.IsNullOrEmpty(prop.Name) || string.IsNullOrEmpty(prop.Type)) continue;
                var property = PropertyDeclaration(ParseTypeName(FindType(attr.AttributeType)), attr.AttributeName)
                    .AddAccessorListAccessors(new AccessorDeclarationSyntax[]
                    {
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                            AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    })
                    .NormalizeWhitespace();
                if (ks.KeyType == HashType)
                {
                    System.Console.WriteLine("Adding annotation...");
                    System.Console.WriteLine(HashKeyToken.ToFullString());
                    property = property.AddAttributeLists(
                        AttributeList(SingletonSeparatedList<AttributeSyntax>(
                            Attribute(HashKeyToken)
                        )
                    )
                    .NormalizeWhitespace());
                }
                else if (ks.KeyType == RangeType)
                {
                    property = property.AddAttributeLists(
                        AttributeList(SingletonSeparatedList<AttributeSyntax>(
                            Attribute(RangeKeyToken)
                        )
                    )
                    .NormalizeWhitespace());
                }
                properties.Add(property);
            }

            classDeclaration = classDeclaration.AddMembers(properties.ToArray()).NormalizeWhitespace();
            code = code.AddMembers(classDeclaration).NormalizeWhitespace();
            System.Console.WriteLine(code.ToFullString());
            return code.ToFullString();
        }
    }
}