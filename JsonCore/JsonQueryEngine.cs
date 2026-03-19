using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace JsonCore
{
    public static class JsonQueryEngine
    {
        // Public entry point: returns 0..N results (wildcards can yield many)
        public static IReadOnlyList<JsonNode?> Evaluate(JsonNode root, string query)
        {
            if (root is null) throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("Query is empty.");

            var tokens = Tokenize(query.Trim());
            return Walk(root, tokens);
        }

        // ----------------------------
        // Token model
        // ----------------------------
        private enum TokenKind { Root, Property, Index, WildcardIndex }

        private readonly record struct Token(TokenKind Kind, string? Name, int Index);

        // ----------------------------
        // Tokenizer
        // ----------------------------
        private static List<Token> Tokenize(string q)
        {
            int i = 0;

            // Must start with $
            SkipWs(q, ref i);
            if (i >= q.Length || q[i] != '$')
                throw new FormatException("Query must start with '$'.");

            var tokens = new List<Token> { new Token(TokenKind.Root, null, -1) };
            i++; // consume $

            while (true)
            {
                SkipWs(q, ref i);
                if (i >= q.Length) break;

                if (q[i] == '.')
                {
                    i++; // consume .
                    SkipWs(q, ref i);
                    var name = ReadIdentifier(q, ref i);
                    if (string.IsNullOrEmpty(name))
                        throw new FormatException("Expected property name after '.'");
                    tokens.Add(new Token(TokenKind.Property, name, -1));
                    continue;
                }

                if (q[i] == '[')
                {
                    i++; // consume [
                    SkipWs(q, ref i);

                    // wildcard [*]
                    if (i < q.Length && q[i] == '*')
                    {
                        i++;
                        SkipWs(q, ref i);
                        Expect(q, ref i, ']');
                        tokens.Add(new Token(TokenKind.WildcardIndex, null, -1));
                        continue;
                    }

                    // quoted property: ['name'] or ["name"]
                    if (i < q.Length && (q[i] == '\'' || q[i] == '"'))
                    {
                        char quote = q[i++];
                        var sb = new StringBuilder();
                        while (i < q.Length && q[i] != quote)
                        {
                            // very small escape handling
                            if (q[i] == '\\' && i + 1 < q.Length)
                            {
                                i++;
                                sb.Append(q[i++]);
                            }
                            else
                            {
                                sb.Append(q[i++]);
                            }
                        }
                        if (i >= q.Length) throw new FormatException("Unterminated string in bracket.");
                        i++; // consume quote
                        SkipWs(q, ref i);
                        Expect(q, ref i, ']');

                        var name = sb.ToString();
                        if (name.Length == 0) throw new FormatException("Empty property name in brackets.");
                        tokens.Add(new Token(TokenKind.Property, name, -1));
                        continue;
                    }

                    // numeric index: [0]
                    var idxText = ReadUntil(q, ref i, ']');
                    if (!int.TryParse(idxText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx))
                        throw new FormatException($"Invalid array index: [{idxText}]");

                    Expect(q, ref i, ']');
                    tokens.Add(new Token(TokenKind.Index, null, idx));
                    continue;
                }

                throw new FormatException($"Unexpected character '{q[i]}' at position {i}.");
            }

            return tokens;
        }

        private static void SkipWs(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static void Expect(string s, ref int i, char expected)
        {
            if (i >= s.Length || s[i] != expected)
                throw new FormatException($"Expected '{expected}'.");
            i++;
        }

        private static string ReadIdentifier(string s, ref int i)
        {
            int start = i;
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                {
                    i++;
                    continue;
                }
                break;
            }
            return s.Substring(start, i - start);
        }

        private static string ReadUntil(string s, ref int i, char terminator)
        {
            int start = i;
            while (i < s.Length && s[i] != terminator) i++;
            if (i >= s.Length) throw new FormatException($"Missing '{terminator}'.");
            return s.Substring(start, i - start);
        }

        // ----------------------------
        // Evaluator
        // ----------------------------
        private static List<JsonNode?> Walk(JsonNode root, List<Token> tokens)
        {
            // Start with one “current” item: root
            var current = new List<JsonNode?> { root };

            for (int t = 0; t < tokens.Count; t++)
            {
                var token = tokens[t];
                if (token.Kind == TokenKind.Root)
                    continue; // already at root

                var next = new List<JsonNode?>();

                foreach (var node in current)
                {
                    if (node is null) continue;

                    switch (token.Kind)
                    {
                        case TokenKind.Property:
                            if (node is JsonObject obj)
                            {
                                obj.TryGetPropertyValue(token.Name!, out JsonNode? value);
                                next.Add(value);
                            }
                            else
                            {
                                next.Add(null);
                            }
                            break;

                        case TokenKind.Index:
                            if (node is JsonArray arr)
                            {
                                if (token.Index >= 0 && token.Index < arr.Count)
                                    next.Add(arr[token.Index]);
                                else
                                    next.Add(null);
                            }
                            else
                            {
                                next.Add(null);
                            }
                            break;

                        case TokenKind.WildcardIndex:
                            if (node is JsonArray arr2)
                            {
                                for (int k = 0; k < arr2.Count; k++)
                                    next.Add(arr2[k]);
                            }
                            else if (node is JsonObject obj2)
                            {
                                // Optional: wildcard over object values
                                foreach (var kv in obj2)
                                    next.Add(kv.Value);
                            }
                            else
                            {
                                // nothing
                            }
                            break;
                    }
                }

                current = next;
            }

            return current;
        }
    }
}
