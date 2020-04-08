﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using ToolGood.Words.internals;

//namespace ToolGood.Words
//{
//    /// <summary>
//    /// 文本搜索, 支持
//    /// </summary>
//    public class StringMatch
//    {
//        private TrieNode3[] _first = new TrieNode3[char.MaxValue + 1];
//        internal int[] _keywordLength;
//        internal int[] _keywordIndex;
//        internal string[] _keywords;


//        /// <summary>
//        /// 设置关键字
//        /// </summary>
//        /// <param name="keywords">关键字列表</param>
//        public virtual void SetKeywords(ICollection<string> keywords)
//        {
//            _keywords = keywords.ToArray();
//            List<string> newKeyword = new List<string>();
//            List<int> newKeywordLength = new List<int>();
//            List<int> newKeywordIndex = new List<int>();
//            var index = 0;
//            foreach (var keyword in keywords) {
//                if (HasMatch(keyword) == false) {
//                    newKeyword.Add(keyword);
//                    newKeywordLength.Add(keyword.Length);
//                    newKeywordIndex.Add(index);
//                } else {
//                    var list = MatchKeywordBuild(keyword);
//                    foreach (var item in list) {
//                        newKeyword.Add(item);
//                        newKeywordLength.Add(item.Length);
//                        newKeywordIndex.Add(index);
//                    }
//                }
//                index++;
//            }
//            _keywordLength = newKeywordLength.ToArray();
//            _keywordIndex = newKeywordIndex.ToArray();

//            SetKeywords2(newKeyword);
//        }
//        private void SetKeywords2(List<string> keywords)
//        {
//            List<TrieNode> allNode = BuildFirstLayerTrieNode(keywords);
//            TrieNode root = allNode[0];


//            var allNode2 = new List<TrieNode3>();
//            for (int i = 0; i < allNode.Count; i++) {
//                allNode2.Add(new TrieNode3());
//            }
//            for (int i = 0; i < allNode2.Count; i++) {
//                var oldNode = allNode[i];
//                var newNode = allNode2[i];

//                foreach (var item in oldNode.m_values) {
//                    var key = item.Key;
//                    var index = item.Value.Index;
//                    if (key == 0) {
//                        newNode.HasWildcard = true;
//                        newNode.WildcardNode = allNode2[index]; //有没有两个重叠 的 通配符
//                    } else {
//                        newNode.Add(key, allNode2[index]);
//                    }
//                }
//                foreach (var item in oldNode.Results) {
//                    newNode.SetResults(item);
//                }
//                if (oldNode.Failure != root) {
//                    foreach (var item in oldNode.Failure.m_values) {
//                        var key = item.Key;
//                        var index = item.Value.Index;
//                        if (key == 0) {
//                            newNode.HasWildcard = true;
//                            newNode.WildcardNode = allNode2[index];
//                        } else if (newNode.HasKey(key) == false) {
//                            newNode.Add(key, allNode2[index]);
//                        }
//                    }
//                    foreach (var item in oldNode.Failure.Results) {
//                        newNode.SetResults(item);
//                    }
//                }
//            }
//            allNode.Clear();
//            allNode = null;
//            root = null;

//            TrieNode3[] first = new TrieNode3[char.MaxValue + 1];
//            foreach (var item in allNode2[0].m_values) {
//                first[item.Key] = item.Value;
//            }
//            _first = first;
//        }

//        private List<TrieNode> BuildFirstLayerTrieNode(List<string> keywords)
//        {
//            var root = new TrieNode();

//            Dictionary<int, List<TrieNode>> allNodeLayers = new Dictionary<int, List<TrieNode>>();

//            #region 第一次关键字
//            for (int i = 0; i < keywords.Count; i++) {
//                var p = _keywords[i];
//                var nd = root;
//                var start = 0;
//                while (p[start] == 0) { // 0 为 通配符
//                    start++;
//                }
//                for (int j = start; j < p.Length; j++) {
//                    nd = nd.Add((char)p[j]);
//                    if (nd.Layer == 0) {
//                        nd.Layer = j + 1 - start;
//                        List<TrieNode> trieNodes;
//                        if (allNodeLayers.TryGetValue(nd.Layer, out trieNodes) == false) {
//                            trieNodes = new List<TrieNode>();
//                            allNodeLayers[nd.Layer] = trieNodes;
//                        }
//                        trieNodes.Add(nd);
//                    }
//                }
//                nd.SetResults(i);
//            }
//            #endregion

//            #region 第二次关键字 通配符
//            for (int i = 0; i < keywords.Count; i++) {
//                var p = _keywords[i];
//                if (p.Contains((char)0) == false) {
//                    continue;
//                }
//                var start = 0;
//                while (p[start] == 0) { // 0 为 通配符
//                    start++;
//                }
//                List<TrieNode> trieNodes = new List<TrieNode>() { root };

//                for (int j = start; j < p.Length; j++) {
//                    List<TrieNode> newTrieNodes = new List<TrieNode>();
//                    var c = p[j];
//                    if (c == 0) {
//                        foreach (var nd in trieNodes) {
//                            newTrieNodes.AddRange(nd.m_values.Values);
//                        }
//                    } else {
//                        foreach (var nd in trieNodes) {
//                            var nd2 = nd.Add(c);
//                            if (nd2.Layer == 0) {
//                                nd2.Layer = j + 1 - start;
//                                List<TrieNode> tnodes;
//                                if (allNodeLayers.TryGetValue(nd2.Layer, out tnodes) == false) {
//                                    tnodes = new List<TrieNode>();
//                                    allNodeLayers[nd.Layer] = tnodes;
//                                }
//                                tnodes.Add(nd2);
//                            }
//                            newTrieNodes.Add(nd2);
//                        }
//                    }
//                    trieNodes = newTrieNodes;
//                }
//                foreach (var nd in trieNodes) {
//                    nd.SetResults(i);
//                }
//            }
//            #endregion

//            #region 添加到 allNode
//            var allNode = new List<TrieNode>();
//            allNode.Add(root);
//            foreach (var trieNodes in allNodeLayers) {
//                foreach (var nd in trieNodes.Value) {
//                    allNode.Add(nd);
//                }
//            }
//            allNodeLayers = null;
//            #endregion

//            #region 第一次 Set Failure
//            for (int i = 1; i < allNode.Count; i++) {
//                var nd = allNode[i];
//                nd.Index = i;
//                TrieNode r = nd.Parent.Failure;
//                char c = nd.Char;
//                while (r != null && !r.m_values.ContainsKey(c)) r = r.Failure;
//                if (r == null)
//                    nd.Failure = root;
//                else {
//                    nd.Failure = r.m_values[c];
//                    foreach (var result in nd.Failure.Results)
//                        nd.SetResults(result);
//                }
//            }
//            #endregion

//            #region 第二次 Set Failure
//            for (int i = 1; i < allNode.Count; i++) {
//                var nd = allNode[i];
//                if (nd.Layer == 1) { continue; }
//                if (nd.Parent.IsWildcard || nd.Char == 0) {
//                    nd.IsWildcard = true;
//                    nd.Failure = root;
//                }
//            }
//            #endregion
//            return allNode;
//        }


//        #region MatchKeywordBuild
//        private bool HasMatch(string keyword)
//        {
//            for (int i = 0; i < keyword.Length; i++) {
//                char c = keyword[i];
//                if (c == '.' || c == '?' || c == '\\' || c == '[') {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private List<string> MatchKeywordBuild(string keyword)
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            Dictionary<int, List<string>> parameterDict = new Dictionary<int, List<string>>();
//            SeparateParameters(keyword, stringBuilder, parameterDict);

//            if (parameterDict.Count == 0) {
//                return new List<string>() { stringBuilder.ToString() };
//            }
//            List<string> parameters = new List<string>();
//            KeywordBuild(parameterDict, 0, parameterDict.Keys.Count - 1, "", parameters);
//            var keywordFmt = stringBuilder.ToString();
//            HashSet<string> list = new HashSet<string>();
//            foreach (var item in parameters) {
//                list.Add(string.Format(keywordFmt, item.Split((char)1)));
//            }
//            return list.ToList();
//        }

//        private void SeparateParameters(string keyword, StringBuilder stringBuilder, Dictionary<int, List<string>> parameterDict)
//        {
//            var index = 0;
//            var parameterIndex = 0;

//            while (index < keyword.Length) {
//                var c = keyword[index];
//                if (c == '.') {
//                    if (index + 1 < keyword.Length && keyword[index + 1] == '?') {
//                        parameterDict[parameterIndex] = new List<string>() { "", ((char)0).ToString() };
//                        stringBuilder.Append("{" + parameterIndex + "}");
//                        parameterIndex++;
//                        index += 2;

//                    } else {
//                        stringBuilder.Append(((char)0));
//                        index++;
//                    }
//                } else if (c == '\\') {
//                    if (index + 2 < keyword.Length && keyword[index + 2] == '?') {
//                        parameterDict[parameterIndex] = new List<string>() { "", keyword[index + 1].ToString() };
//                        stringBuilder.Append("{" + parameterIndex + "}");
//                        parameterIndex++;
//                        index += 3;
//                    } else if (index + 1 < keyword.Length) {
//                        stringBuilder.Append(keyword[index + 1]);
//                        index += 2;
//                    } else {
//                        throw new Exception($"【{keyword}】出错了，最后一位为\\");
//                    }
//                } else if (c == '[') {
//                    index++;
//                    var ps = new List<string>();
//                    while (index < keyword.Length) {
//                        c = keyword[index];
//                        if (c == ']') {
//                            break;
//                        } else if (c == '\\') {
//                            if (index + 1 < keyword.Length) {
//                                ps.Add(keyword[index + 1].ToString());
//                                index += 2;
//                            }
//                        } else {
//                            ps.Add(c.ToString());
//                            index++;
//                        }
//                    }
//                    if (c != ']') {
//                        throw new Exception($"【{keyword}】出错了，最后一位不为]");
//                    }
//                    if (index + 1 < keyword.Length && keyword[index + 1] == '?') {
//                        ps.Insert(0, "");
//                        parameterDict[parameterIndex] = ps;
//                        stringBuilder.Append("{" + parameterIndex + "}");
//                        parameterIndex++;
//                        index += 2;
//                    } else {
//                        parameterDict[parameterIndex] = ps;
//                        stringBuilder.Append("{" + parameterIndex + "}");
//                        parameterIndex++;
//                        index++;
//                    }
//                } else {
//                    if (index + 1 < keyword.Length && keyword[index + 1] == '?') {
//                        parameterDict[parameterIndex] = new List<string>() { "", c.ToString() };
//                        stringBuilder.Append("{" + parameterIndex + "}");
//                        parameterIndex++;
//                        index += 2;
//                    } else {
//                        if (c == '{') {
//                            stringBuilder.Append("{{");
//                        } else if (c == '}') {
//                            stringBuilder.Append("}}");
//                        } else {
//                            stringBuilder.Append(c);
//                        }
//                        index++;
//                    }
//                }
//            }
//        }

//        private void KeywordBuild(Dictionary<int, List<string>> parameterDict, int index, int end, string keyword, List<string> result)
//        {
//            const char span = (char)1;
//            var list = parameterDict[index];
//            if (index == end) {
//                foreach (var item in list) {
//                    result.Add((keyword + span + item).Substring(1));
//                }
//            } else {
//                foreach (var item in list) {
//                    KeywordBuild(parameterDict, index + 1, end, keyword + span + item, result);
//                }
//            }
//        }

//        #endregion


//    }
//}
