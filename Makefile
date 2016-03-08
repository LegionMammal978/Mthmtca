all:
	mcs -reference:Wolfram.NETLink.dll,System.Numerics.dll -out:mthmtca Program.cs
	which MathKernel>kernel_pos.txt
