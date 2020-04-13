/*
Copyright (C) 2016  LegionMammal978

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using Wolfram.NETLink;

[assembly: AssemblyVersion("0.1")]

namespace Mthmtca
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!File.Exists("kernel_pos.txt"))
            {
                Console.Error.WriteLine("Error: Could not find kernel_pos.txt");
                return 2;
            }
            IKernelLink link = MathLinkFactory.CreateKernelLink(String.Format("-linkmode launch -linkname \"{0}\"", File.ReadAllText("kernel_pos.txt").Replace("\n", "")));
            link.WaitAndDiscardAnswer();
            link.Evaluate(new Expr(new Expr(ExpressionType.Symbol, "Names"), new Expr(ExpressionType.String, "*")));
            link.WaitForAnswer();
            string[] names = link.GetStringArray();
            if (args.Length > 1 && (args[0].Equals("-s") || args[0].Equals("--symbol")))
            {
                int pos = Array.IndexOf(names, args[1]);
                if (pos == -1)
                    Console.WriteLine("Symbol '{0}' not found.", args[1]);
                else
                    Console.WriteLine("Symbol '{0}' is at index {1} (0x{1:X4}).", args[1], pos, pos);
                return 0;
            }
            if (args.Length != 0 && !File.Exists(args[0]))
            {
                Console.Error.WriteLine("Error: Could not read input file");
                return 1;
            }
            Stream prog = args.Length != 0 ? File.OpenRead(args[0]) : Console.OpenStandardInput();
            link.Evaluate(GetExpr(prog, names));
            link.WaitForAnswer();
            Console.WriteLine(link.GetString());
            link.Close();
            prog.Close();
            return 0;
        }

        static Expr GetExpr(Stream prog, string[] names)
        {
            short cur = ReadInt16(prog);
            Expr res;
            switch (cur & 0xC000)
            {
                case 0x0000:
                    Expr head = new Expr(ExpressionType.Symbol, names[cur & 0x3FFF]);
                    object[] args = new object[prog.ReadByte()];
                    for (int i = 0; i < args.Length; i++)
                        args[i] = GetExpr(prog, names);
                    res = new Expr(head, args);
                    break;
                case 0x4000:
                    BigInteger n = BigInteger.Zero;
                    int sign = 1 - ((cur & 0x1000) >> 12);
                    if ((cur & 0x2000) > 0)
                        n = sign * (cur & 0xFFF);
                    else
                        for (int i = 0, len = cur & 0xFFF; i < len; i++)
                        {
                            n *= 256;
                            n += sign * prog.ReadByte();
                        }
                    res = new Expr(ExpressionType.Integer, n.ToString());
                    break;
                case 0x8000:
                    char[] arr = new char[cur & 0x3FFF];
                    for (int i = 0; i < arr.Length; i++)
                        arr[i] = (char)prog.ReadByte();
                    res = new Expr(ExpressionType.String, new String(arr));
                    break;
                case 0xC000:
                    res = new Expr(ExpressionType.Symbol, names[cur & 0x3FFF]);
                    break;
                default:
                    res = new Expr(ExpressionType.Symbol, "Null");
                    break;
            }
            return res;
        }

        static short ReadInt16(Stream prog) { return (short)((prog.ReadByte() << 8) | prog.ReadByte()); }
    }
}
