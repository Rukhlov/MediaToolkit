using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Jupiter
{
    public enum StateFlag
    {
        // ControlPoint Protocol Manual p. 122
        //Get -Set Flags

        wsVisible = 0x0001,
        wsMinimized = 0x0002,
        wsMaximized = 0x0004,
        wsFramed = 0x0008,
        wsLockAspect = 0x0010,
        wsAlwaysOnTop = 0x0020,
        wsPosition = 0x0400,
        wsSize = 0x0800,
        wsZOrder = 0x1000,
        wsTitle = 0x2000,

        //Notify Flags (read only)
        wsKind = 0x4000,
        wsChannel = 0x00010000,
        wsBalance = 0x00020000,
        wsFormat = 0x00040000,
        wsCrop = 0x00080000,

    }

    public enum SubSystemKind
    { // ControlPoint Protocol Manual p. 48
        None = 0,
        Galileo = 1,
        LiveVideo = 2,
        RGBCapture = 3,
        SystemWindow = 4,
        CPShare = 5,
        VidStream = 6,
        CPWeb = 7,
        PictureViewer = 8,
        CatalystLink = 9,
        IPStream = 10,
    }

    public class WinId
    {// ControlPoint Protocol Manual p. 50

        internal WinId(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            if (valueList.Count == 1)
            {
                var _id = valueList[0];
                Id = int.Parse(_id);
            }
            else
            {
                throw new FormatException("WinId invalid argsTree");
            }
        }

        public WinId(int id)
        {
            Id = id;
        }

        // >0 and <10,000 for User Defined ID numbers
        //>10,000 for ControlPoint generated ID numbers
        public int Id { get; set; } = -1;

        public override string ToString()
        {
            return "{ " + Id + " }";
        }

    }

    public class WinIdList : List<WinId>
    {
        //"{ 3 { 10001 } { 10002 } { 123 } }"

        public WinIdList(string data) : this(TreeNode.Parse(data))
        { }

        public WinIdList(TreeNode argsTree)
        {

            var valueList = argsTree.ValueList;
            if (valueList.Count == 1)
            {
                var _count = valueList[0];
                var count = int.Parse(_count);
                var nodes = argsTree.Nodes;

                if (nodes.Count == count)
                {
                    foreach (var n in nodes)
                    {
                        var winId = new WinId(n);
                        this.Add(winId);
                    }
                }
            }
            else
            {
                throw new FormatException("WinId invalid argsTree");
            }
        }

        public override string ToString()
        {
            return "{ " + this.Count + " " + string.Join(" ", this) + " }";
        }
    }

    public class TWindowState
    {
        // { { 101 } 2 9 7183 154 292 320 240 { 10003 } }

        public WinId Id { get; private set; } = new WinId(-1);
        public SubSystemKind Kind { get; private set; } = SubSystemKind.None;
        public uint State { get; private set; } = 0;
        public uint StateChange { get; private set; } = 0;
        public int x { get; private set; } = 0;
        public int y { get; private set; } = 0;
        public int w { get; private set; } = 0;
        public int h { get; private set; } = 0;
        public WinId ZAfter { get; private set; } = new WinId(-1);

        public TWindowState(string data) : this(TreeNode.Parse(data))
        { }

        public TWindowState(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 2 && valueList.Count == 7)
            {
                this.Id = new WinId(nodes[0]);
                this.ZAfter = new WinId(nodes[1]);

                this.Kind = (SubSystemKind)int.Parse(valueList[0]);

                this.State = IntParser.Parse<uint>(valueList[1]);
                this.StateChange = IntParser.Parse<uint>(valueList[2]);

                this.x = int.Parse(valueList[3]);
                this.y = int.Parse(valueList[4]);
                this.w = int.Parse(valueList[5]);
                this.h = int.Parse(valueList[6]);

            }
            else
            {
                throw new ArgumentException("argsTree");
            }

        }

        public override string ToString()
        {
            return "{ " + string.Join(" ", Id, (int)Kind, State, StateChange, x, y, w, h, ZAfter) + " }";
        }

    }


    public class TWindowStateList : List<TWindowState>
    {
        //"{ 2 { { 101 } 2 9 7183 154 292 320 240 { 10003 } } { { 102 } 3 1 7183 152 21 320 240 { 102 } } }

        public TWindowStateList(string data) : this(TreeNode.Parse(data))
        { }

        public TWindowStateList(TreeNode argsTree)
        {

            var valueList = argsTree.ValueList;
            if (valueList.Count == 1)
            {
                var _count = valueList[0];
                var count = int.Parse(_count);
                var nodes = argsTree.Nodes;

                if (nodes.Count == count)
                {
                    foreach (var n in nodes)
                    {
                        var winId = new TWindowState(n);
                        this.Add(winId);
                    }
                }
            }
            else
            {
                throw new FormatException("TWindowStateList invalid argsTree");
            }
        }

        public override string ToString()
        {
            return "{ " + this.Count + " " + string.Join(" ", this) + " }";
        }
    }

    public class TreeNode
    {
        public List<string> ValueList { get; private set; } = new List<string>();
        public List<TreeNode> Nodes { get; private set; } = new List<TreeNode>();


        private int offset = 0;
        private StringBuilder buffer = new StringBuilder();

        private void Setup()
        {
            var valueList = buffer.ToString();
            valueList = valueList.TrimStart(' ');
            valueList = valueList.TrimEnd(' ');

            // удаляем двойные пробелы...
            valueList = System.Text.RegularExpressions.Regex.Replace(valueList, @"\s+", " ");

            this.ValueList = new List<string>(valueList.Split(' '));
        }

        public static TreeNode Parse(string data)
        {
            data = data.TrimStart(' ');
            data = data.TrimEnd(' ');

            if (!Validate(data))
            {
                throw new ArgumentException(data);
            }

            return Parse(data.ToArray());

        }

        public static bool Validate(string data)
        {
            if (!data.StartsWith("{") || !data.EndsWith("}"))
            {
                return false;
            }

            // проверяем незакрытые скобки...
            bool result = false;
            Stack<char> stack = new Stack<char>();

            char[] chars = data.ToCharArray();
            for (int i = 0; i < data.Length; i++)
            {
                if (chars[i] == '{')
                {
                    stack.Push(chars[i]);
                }
                else if (chars[i] == '}')
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (stack.Count == 0)
            {
                result = true;
            }

            return result;

        }

        private static TreeNode Parse(char[] data, int index = 0)
        {// TODO: переделать без рекурсии...

            TreeNode node = new TreeNode();

            index++;

            for (int i = index; i < data.Length; i++)
            {
                node.offset++;
                if (data[i] == '}')
                {
                    node.Setup();
                    return node;
                }

                if (data[i] == '{')
                {
                    TreeNode child = Parse(data, i);

                    i += child.offset;
                    node.offset += child.offset;

                    node.Nodes.Add(child);
                }
                else
                {
                    node.buffer.Append(data[i]);
                }

            }
            return node;
        }

    }


    static class IntParser
    {
        private static Dictionary<Type, Func<string, NumberStyles, object>> dict = new Dictionary<Type, Func<string, NumberStyles, object>>
        {
            { typeof(byte),   (s, style) => byte.Parse(s, style) },
            { typeof(sbyte),  (s, style) => sbyte.Parse(s, style) },
            { typeof(short),  (s, style) => short.Parse(s, style) },
            { typeof(ushort), (s, style) => ushort.Parse(s, style) },
            { typeof(int),    (s, style) => int.Parse(s, style) },
            { typeof(uint),   (s, style) => uint.Parse(s, style) },
            { typeof(long),   (s, style) => long.Parse(s, style) },
            { typeof(ulong),  (s, style) => ulong.Parse(s, style) },
        };

        public static T Parse<T>(string str) where T : IComparable, IFormattable, IConvertible
        {
            var parser = dict[typeof(T)];

            var style = NumberStyles.Integer;

            if (str.StartsWith("0x"))
            {
                style = NumberStyles.HexNumber;
                str = str.Replace("0x", "");
            }

            T val = (T)parser(str, style);

            return val;
        }

    }

}
