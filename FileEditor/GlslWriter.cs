using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HlslDecompiler.DirectXShaderModel;
using HlslDecompiler.Hlsl;
using HlslDecompiler.Util;

namespace HlslDecompiler
{
    // Standalone GLSL writer — does not depend on HlslWriter
    public class GlslWriter
    {
        private readonly ShaderModel _shader;
        private int _loopVariableIndex = -1;
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private Dictionary<RegisterKey, int> _registerWriteMasks;

        TextWriter internalWriter;
        string indent = "";

        HlslAst _ast;
        RegisterState _registers;

        public GlslWriter(ShaderModel shader)
        {
            _shader = shader;
        }

        // Public helpers to write to a path or TextWriter
        public void Write(string glslFilename)
        {
            using var file = new FileStream(glslFilename, FileMode.Create, FileAccess.Write);
            using (var writer = new StreamWriter(file))
            {
                Write(writer);
                writer.Dispose();
            }
            file.Dispose();
        }

        public void Write(TextWriter writer)
        {
            internalWriter = writer;
            WriteLine("#version 330");
            WriteInternal();
        }
        private string ModifyType(string hlslType)
        {
            return hlslType switch
            {
                "float2" => "vec2",
                "float3" => "vec3",
                "float4" => "vec4",
                "float4x4" => "mat4",
                "float3x4" => "mat3x4",
                var c => c
            };
        }
        private void WriteInternal()
        {
            _ast = InstructionParser.Parse(_shader);
            _registers = _ast.RegisterState;

            WriteConstantDeclarations();
            WriteResourceDeclarations();

            // Declare inputs/outputs as GLSL 'in'/'out'
            if (_registers.MethodInputRegisters.Count > 0)
            {
                if (_shader.Type == ShaderType.Pixel)
                {
                    WriteLine("in VertexData  {");
                    foreach (var input in _registers.MethodInputRegisters.Values)
                    {
                        WriteLine("{0} {1};", ModifyType(input.TypeName), input.Name);
                    }
                    WriteLine("} i;");
                }
                else
                {
                    foreach (var input in _registers.MethodInputRegisters.Values)
                    {
                        WriteLine("in {0} {1};", ModifyType(input.TypeName), input.Name);
                    }
                }
                WriteLine();
            }

            if (_registers.MethodOutputRegisters.Count > 0)
            {
                IList<RegisterDeclaration> outputs = _registers.MethodOutputRegisters;
                if(_shader.Type == ShaderType.Vertex)
                {
                    WriteLine("out VertexBlock {");
                    foreach (var output in outputs)
                    {
                        WriteLine("{0} {1};", ModifyType(output.TypeName), output.Name);
                    }
                    WriteLine("} o;");
                }
                else
                {
                    foreach (var output in outputs)
                    {
                        WriteLine("out {0} {1};", ModifyType(output.TypeName), output.Name);
                    }
                }
                WriteLine();
            }

            WriteLine("void main()");
            WriteLine("{");
            indent = "\t";

            WriteMethodBody();

            indent = "";
            WriteLine("}");
        }

        private void WriteConstantDeclarations()
        {
            if (_registers.ConstantDeclarations.Count == 0)
            {
                return;
            }

            var compiler = new ConstantDeclarationCompiler();

            foreach (ConstantDeclaration declaration in _registers.ConstantDeclarations)
            {
                compiler.SetStructOrder(declaration);
            }

            IList<ShaderTypeInfo> structs = compiler.GetOrderedStructs();
            for (int i = 0; i < structs.Count; i++)
            {
                WriteLine($"struct struct{i + 1}");
                WriteLine("{");
                indent = "\t";
                foreach (var member in structs[i].MemberInfo)
                {
                    WriteLine(compiler.Compile(member,ModifyType));
                }
                indent = "";
                WriteLine("};");
                WriteLine();
            }

            foreach (ConstantDeclaration declaration in _registers.ConstantDeclarations)
            {
                // Emit as uniform block or simple uniform, keep original compiler output but prefix with 'uniform'
                string decl = compiler.Compile(declaration, ModifyType);
                // If compiler produced a struct declaration followed by name, try to convert to uniform
                // Example: "float4 MyConst : register(c0)" -> convert to "uniform float4 MyConst;"
                // We simply prefix with 'uniform' and strip any HLSL semantics that compiler may produce.
                decl = decl.Split(new[] { ':' }, 2)[0].Trim();
                if (!decl.EndsWith(";")) decl += ";";
                WriteLine("uniform {0}", decl);
            }
            WriteLine();
        }

        private void WriteResourceDeclarations()
        {
            if (_registers.ResourceDefinitions == null || _registers.ResourceDefinitions.Count == 0)
            {
                return;
            }

            foreach (var resource in _registers.ResourceDefinitions)
            {
                if (resource.ShaderInputType == D3DShaderInputType.Texture)
                {
                    // Map Dimension to GLSL sampler type (best-effort)
                    string samplerType = "sampler2D";
                    switch (resource.Dimension)
                    {
                        case ResourceDimension.Texture1D:
                            samplerType = "sampler1D";
                            break;
                        case ResourceDimension.Texture2D:
                            samplerType = "sampler2D";
                            break;
                        case ResourceDimension.Texture3D:
                            samplerType = "sampler3D";
                            break;
                        case ResourceDimension.TextureCube:
                            samplerType = "samplerCube";
                            break;
                    }
                    WriteLine("uniform {0} {1};", ModifyType(samplerType), resource.Name);
                }
                else if (resource.ShaderInputType == D3DShaderInputType.Sampler)
                {
                    // GLSL combines sampler + state; map to sampler2D as a fallback
                    WriteLine("uniform sampler2D {0};", resource.Name);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            WriteLine();
        }

        protected void WriteLine()
        {
            internalWriter.WriteLine();
        }

        protected void WriteLine(string value)
        {
            internalWriter.Write(indent);
            internalWriter.WriteLine(value);
        }

        protected void WriteLine(string format, params object[] args)
        {
            internalWriter.Write(indent);
            internalWriter.WriteLine(format, args);
        }

        // The GLSL method body is adapted from the HLSL->GLSL conversion rules
        private void WriteMethodBody()
        {
            _registerWriteMasks = FindTemporaryRegisterAssignments(_shader.Instructions);
            WriteTemporaryVariableDeclarations();

            foreach (Instruction instruction in _shader.Instructions)
            {
                if (instruction is D3D9Instruction d3d9Instruction)
                {
                    WriteInstruction(d3d9Instruction);
                }
                else if (instruction is D3D10Instruction d9d10Instruction)
                {
                    WriteInstruction(d9d10Instruction);
                }
            }
        }

        private void WriteTemporaryVariableDeclarations()
        {
            foreach (var register in _registerWriteMasks)
            {
                int writeMask = register.Value;
                string writeMaskName;
                switch (writeMask)
                {
                    case 0x1:
                        writeMaskName = "float";
                        break;
                    case 0x3:
                        writeMaskName = "vec2";
                        break;
                    case 0x7:
                        writeMaskName = "vec3";
                        break;
                    case 0xF:
                        writeMaskName = "vec4";
                        break;
                    default:
                        writeMaskName = "vec4";
                        break;
                }
                WriteLine("{0} {1};", writeMaskName, GetTempRegisterName(register.Key));
            }
            if (_registerWriteMasks.Count > 0) WriteLine();
        }

        private static Dictionary<RegisterKey, int> FindTemporaryRegisterAssignments(IList<Instruction> instructions)
        {
            var tempRegisters = new Dictionary<RegisterKey, int>();
            foreach (Instruction instruction in instructions.Where(i => i.HasDestination))
            {
                int destIndex = instruction.GetDestinationParamIndex();
                if (instruction is D3D9Instruction d3D9Instruction
                    && (d3D9Instruction.GetParamRegisterType(destIndex) == RegisterType.Temp
                    || d3D9Instruction.GetParamRegisterType(destIndex) == RegisterType.Addr))
                {
                    int writeMask = instruction.GetDestinationWriteMask();

                    var registerKey = instruction.GetParamRegisterKey(destIndex);
                    if (!tempRegisters.TryAdd(registerKey, writeMask))
                    {
                        tempRegisters[registerKey] |= writeMask;
                    }
                }
                else if (instruction is D3D10Instruction d3D10Instruction
                    && d3D10Instruction.GetParamRegisterKey(destIndex).IsTempRegister)
                {
                    int writeMask = instruction.GetDestinationWriteMask();

                    var registerKey = instruction.GetParamRegisterKey(destIndex);
                    if (!tempRegisters.TryAdd(registerKey, writeMask))
                    {
                        tempRegisters[registerKey] |= writeMask;
                    }
                }
            }
            return tempRegisters;
        }

        private static string GetTempRegisterName(RegisterKey registerKey)
        {
            if (registerKey.IsTempRegister)
            {
                return "r" + registerKey.Number;
            }
            return registerKey.ToString();
        }

        // Build assignment format and wrap with clamp if saturate is present (GLSL uses clamp(x,0.0,1.0))
        private static string GetModifier(D3D9Instruction instruction)
        {
            string source = "{1}";
            ResultModifier resultModifier = instruction.GetDestinationResultModifier();
            if (resultModifier.HasFlag(ResultModifier.Saturate))
            {
                source = $"clamp({source}, 0.0, 1.0)";
            }
            // ignore PartialPrecision for GLSL
            return "{0} = " + source + ";";
        }

        private void WriteInstruction(D3D9Instruction instruction)
        {
            switch (instruction.Opcode)
            {
                case Opcode.Abs:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"abs({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Add:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"{GetSourceName(instruction, 1)} + {GetSourceName(instruction, 2)}");
                    break;
                case Opcode.BreakC:
                    string ifComparisonBreak;
                    switch (instruction.Comparison)
                    {
                        case IfComparison.GT:
                            ifComparisonBreak = ">";
                            break;
                        case IfComparison.EQ:
                            ifComparisonBreak = "==";
                            break;
                        case IfComparison.GE:
                            ifComparisonBreak = ">=";
                            break;
                        case IfComparison.LE:
                            ifComparisonBreak = "<=";
                            break;
                        case IfComparison.NE:
                            ifComparisonBreak = "!=";
                            break;
                        case IfComparison.LT:
                            ifComparisonBreak = "<";
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    WriteLine("if ({0} {2} {1}) break;", GetSourceName(instruction, 0), GetSourceName(instruction, 1), ifComparisonBreak);
                    break;
                case Opcode.Cmp:
                    // TODO: should be per-component
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"({GetSourceName(instruction, 1)} >= 0.0) ? {GetSourceName(instruction, 2)} : {GetSourceName(instruction, 3)}");
                    break;
                case Opcode.DP2Add:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"dot({GetSourceName(instruction, 1)}, {GetSourceName(instruction, 2)}) + {GetSourceName(instruction, 3)}");
                    break;
                case Opcode.Dp3:
                case Opcode.Dp4:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"dot({GetSourceName(instruction, 1)}, {GetSourceName(instruction, 2)})");
                    break;
                case Opcode.DSX:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"dFdx({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.DSY:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"dFdy({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Else:
                    indent = indent.Substring(0, Math.Max(0, indent.Length - 1));
                    WriteLine("} else {");
                    indent += "\t";
                    break;
                case Opcode.Endif:
                    indent = indent.Substring(0, Math.Max(0, indent.Length - 1));
                    WriteLine("}");
                    break;
                case Opcode.EndLoop:
                case Opcode.EndRep:
                    indent = indent.Substring(0, Math.Max(0, indent.Length - 1));
                    WriteLine("}");
                    _loopVariableIndex--;
                    break;
                case Opcode.Exp:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"exp2({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Frc:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"fract({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.If:
                    WriteLine("if ({0}) {{", GetSourceName(instruction, 0));
                    indent += "\t";
                    break;
                case Opcode.IfC:
                    string ifComparison;
                    switch (instruction.Comparison)
                    {
                        case IfComparison.GT:
                            ifComparison = ">";
                            break;
                        case IfComparison.EQ:
                            ifComparison = "==";
                            break;
                        case IfComparison.GE:
                            ifComparison = ">=";
                            break;
                        case IfComparison.LE:
                            ifComparison = "<=";
                            break;
                        case IfComparison.NE:
                            ifComparison = "!=";
                            break;
                        case IfComparison.LT:
                            ifComparison = "<";
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    WriteLine("if ({0} {2} {1}) {{", GetSourceName(instruction, 0), GetSourceName(instruction, 1), ifComparison);
                    indent += "\t";
                    break;
                case Opcode.Log:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"log2({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Loop:
                    ConstantIntRegister intRegister = _registers.FindConstantIntRegister(instruction.GetParamRegisterNumber(1));
                    uint end = intRegister.Value[0];
                    uint start = intRegister.Value[1];
                    uint stride = intRegister.Value[2];
                    _loopVariableIndex++;
                    string loopVariable = "i" + _loopVariableIndex;
                    if (stride == 1)
                    {
                        WriteLine("for (int {2} = {0}; {2} < {1}; {2}++) {{", start, end, loopVariable);
                    }
                    else
                    {
                        WriteLine("for (int {3} = {0}; {3} < {1}; {3} += {2}) {{", start, end, stride, loopVariable);
                    }
                    indent += "\t";
                    break;
                case Opcode.Lrp:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"mix({GetSourceName(instruction, 2)}, {GetSourceName(instruction, 3)}, {GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Mad:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"{GetSourceName(instruction, 1)} * {GetSourceName(instruction, 2)} + {GetSourceName(instruction, 3)}");
                    break;
                case Opcode.Max:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"max({GetSourceName(instruction, 1)}, {GetSourceName(instruction, 2)})");
                    break;
                case Opcode.Min:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"min({GetSourceName(instruction, 1)}, {GetSourceName(instruction, 2)})");
                    break;
                case Opcode.Mov:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case Opcode.MovA:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case Opcode.Mul:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"{GetSourceName(instruction, 1)} * {GetSourceName(instruction, 2)}");
                    break;
                case Opcode.Nrm:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"normalize({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Pow:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"pow({GetSourceName(instruction, 1)}, {GetSourceName(instruction, 2)})");
                    break;
                case Opcode.Rcp:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"1.0 / {GetSourceName(instruction, 1)}");
                    break;
                case Opcode.Rep:
                    ConstantIntRegister loopRegister = _registers.FindConstantIntRegister(instruction.GetParamRegisterNumber(0));
                    _loopVariableIndex++;
                    WriteLine("for (int {1} = 0; {1} < {0}; {1}++) {{", loopRegister[0], "i" + _loopVariableIndex);
                    indent += "\t";
                    break;
                case Opcode.Rsq:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"inversesqrt({GetSourceName(instruction, 1)})");
                    break;
                case Opcode.Sge:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"({GetSourceName(instruction, 1)} >= {GetSourceName(instruction, 2)}) ? 1 : 0");
                    break;
                case Opcode.Slt:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"({GetSourceName(instruction, 1)} < {GetSourceName(instruction, 2)}) ? 1 : 0");
                    break;
                case Opcode.SinCos:
                    // GLSL doesn't have a sincos intrinsic; produce a vec2(sin, cos)
                    WriteLine("{0} = vec2(sin({1}), cos({1}));", GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case Opcode.Sub:
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"{GetSourceName(instruction, 1)} - {GetSourceName(instruction, 2)}");
                    break;
                case Opcode.Tex:
                    if ((_shader.MajorVersion == 1 && _shader.MinorVersion >= 4) || (_shader.MajorVersion > 1))
                    {
                        ConstantDeclaration sampler = _registers.FindConstant(RegisterSet.Sampler, instruction.GetParamRegisterNumber(2));
                        int samplerDimension = sampler.GetSamplerDimension();
                        bool isCube = sampler.TypeInfo.ParameterType == ParameterType.SamplerCube;
                        if (instruction.TexldControls.HasFlag(TexldControls.Project))
                        {
                            WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                                $"textureProj({GetSourceName(instruction, 2)}, {GetSourceName(instruction, 1, 4)})");
                        }
                        else if (instruction.TexldControls.HasFlag(TexldControls.Bias))
                        {
                            WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                                $"textureLod({GetSourceName(instruction, 2)}, {GetSourceName(instruction, 1, 4)}, /*bias*/ 0.0)");
                        }
                        else
                        {
                            WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                                $"texture({GetSourceName(instruction, 2)}, {GetSourceName(instruction, 1, samplerDimension)})");
                        }
                    }
                    else
                    {
                        WriteLine(GetModifier(instruction), GetDestinationName(instruction), "texture()");
                    }
                    break;
                case Opcode.TexLDL:
                {
                    ConstantDeclaration sampler = _registers.FindConstant(RegisterSet.Sampler, instruction.GetParamRegisterNumber(2));
                    int samplerDimension = sampler.GetSamplerDimension();
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"textureLod({GetSourceName(instruction, 2)}, {GetSourceName(instruction, 1, 4)}, /*lod*/ 0.0)");
                    break;
                }
                case Opcode.TexLDD:
                {
                    ConstantDeclaration sampler = _registers.FindConstant(RegisterSet.Sampler, instruction.GetParamRegisterNumber(2));
                    int samplerDimension = sampler.GetSamplerDimension();
                    WriteLine(GetModifier(instruction), GetDestinationName(instruction),
                        $"textureGrad({GetSourceName(instruction, 2)}, {GetSourceName(instruction, 1, samplerDimension)}, {GetSourceName(instruction, 3, samplerDimension)})");
                    break;
                }
                case Opcode.TexKill:
                    // HLSL clip(v) -> GLSL discard if any component < 0.0
                    WriteLine("if (any(lessThan({0}, vec4(0.0)))) discard;", GetDestinationName(instruction));
                    break;
                case Opcode.Def:
                case Opcode.DefB:
                case Opcode.DefI:
                case Opcode.Dcl:
                case Opcode.Comment:
                case Opcode.End:
                    break;
                default:
                    break;
            }
        }

        private void WriteInstruction(D3D10Instruction instruction)
        {
            switch (instruction.Opcode)
            {
                case D3D10Opcode.Add:
                    WriteLine("{0} = {1} + {2};", GetDestinationName(instruction), GetSourceName(instruction, 1), GetSourceName(instruction, 2));
                    break;
                case D3D10Opcode.DerivRtx:
                    WriteLine("{0} = dFdx({1});", GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case D3D10Opcode.DerivRty:
                    WriteLine("{0} = dFdy({1});", GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case D3D10Opcode.Discard:
                    WriteLine("if ({0}) discard;", GetSourceName(instruction, 0));
                    break;
                case D3D10Opcode.Dp2:
                case D3D10Opcode.Dp3:
                case D3D10Opcode.Dp4:
                    WriteLine("{0} = dot({1}, {2});", GetDestinationName(instruction), GetSourceName(instruction, 1), GetSourceName(instruction, 2));
                    break;
                case D3D10Opcode.GE:
                    WriteLine("{0} = ({1} >= {2}) ? -1 : 0;", GetDestinationName(instruction), GetSourceName(instruction, 1), GetSourceName(instruction, 2));
                    break;
                case D3D10Opcode.Mad:
                    WriteLine("{0} = {1} * {2} + {3};", GetDestinationName(instruction),
                        GetSourceName(instruction, 1), GetSourceName(instruction, 2), GetSourceName(instruction, 3));
                    break;
                case D3D10Opcode.Mov:
                    WriteLine("{0} = {1};", GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case D3D10Opcode.MovC:
                    WriteLine("{0} = ({1} != 0) ? {2} : {3};", GetDestinationName(instruction), GetSourceName(instruction, 1), GetSourceName(instruction, 2), GetSourceName(instruction, 3));
                    break;
                case D3D10Opcode.Mul:
                    WriteLine("{0} = {1} * {2};", GetDestinationName(instruction), GetSourceName(instruction, 1), GetSourceName(instruction, 2));
                    break;
                case D3D10Opcode.Rsq:
                    WriteLine("{0} = inversesqrt({1});", GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case D3D10Opcode.Sample:
                    var samplerKey = instruction.GetParamRegisterKey(2);
                    int texCoordLength = _registers.ResourceDefinitions
                        .Where(d => d.ShaderInputType == D3DShaderInputType.Texture)
                        .First(d => d.BindPoint == samplerKey.Number)
                        .GetDimensionSize();
                    WriteLine("{0} = texture({2}, {1});", GetDestinationName(instruction), GetSourceName(instruction, 1, texCoordLength), GetSourceName(instruction, 2));
                    break;
                case D3D10Opcode.Sqrt:
                    WriteLine("{0} = sqrt({1});", GetDestinationName(instruction), GetSourceName(instruction, 1));
                    break;
                case D3D10Opcode.DclConstantBuffer:
                case D3D10Opcode.DclInput:
                case D3D10Opcode.DclInputPS:
                case D3D10Opcode.DclInputPSSiv:
                case D3D10Opcode.DclOutput:
                case D3D10Opcode.DclResource:
                case D3D10Opcode.DclSampler:
                case D3D10Opcode.DclTemps:
                case D3D10Opcode.Ret:
                    break;
                default:
                    break;
            }
        }

        private string GetDestinationName(Instruction instruction)
        {
            int destIndex = instruction.GetDestinationParamIndex();
            RegisterKey registerKey = instruction.GetParamRegisterKey(destIndex);

            string registerName;
            if (instruction is D3D9Instruction d3D9Instruction && d3D9Instruction.Opcode == Opcode.MovA
                && (registerKey as D3D9RegisterKey).Type == RegisterType.Addr)
            {
                registerName = "a0";
            }
            else
            {
                registerName = _registers.GetRegisterNameGLSL(registerKey);
            }
            int registerLength = _registers.GetRegisterMaskedLength(registerKey);
            string writeMaskName = instruction.GetDestinationWriteMaskName(registerLength);

            return string.Format("{0}{1}", registerName, writeMaskName);
        }

        private string GetSourceName(D3D9Instruction instruction, int srcIndex, int? destinationLength = null)
        {
            string sourceRegisterName;

            var registerKey = instruction.GetParamRegisterKey(srcIndex) as D3D9RegisterKey;
            switch (registerKey.Type)
            {
                case RegisterType.Const:
                case RegisterType.Const2:
                case RegisterType.Const3:
                case RegisterType.Const4:
                case RegisterType.ConstBool:
                case RegisterType.ConstInt:
                    string constantValue = GetSourceConstantValue(instruction, srcIndex, destinationLength);
                    if (constantValue != null)
                    {
                        return constantValue;
                    }

                    ConstantDeclaration decl = _registers.FindConstant(registerKey);
                    if (decl == null)
                    {
                        // Constant register not found in def statements nor the constant table
                        throw new NotImplementedException();
                    }

                    if ((decl.TypeInfo.ParameterClass == ParameterClass.MatrixRows && _registers.ColumnMajorOrder) ||
                        (decl.TypeInfo.ParameterClass == ParameterClass.MatrixColumns && !_registers.ColumnMajorOrder))
                    {
                        int row = registerKey.Number - decl.RegisterIndex;
                        sourceRegisterName = $"{decl.Name}[{row}]";
                    }
                    else if ((decl.TypeInfo.ParameterClass == ParameterClass.MatrixColumns && _registers.ColumnMajorOrder) ||
                        (decl.TypeInfo.ParameterClass == ParameterClass.MatrixRows && !_registers.ColumnMajorOrder))
                    {
                        int column = registerKey.Number - decl.RegisterIndex;
                        sourceRegisterName = $"transpose({decl.Name})[{column}]";
                    }
                    else
                    {
                        sourceRegisterName = decl.Name;
                    }
                    break;
                default:
                    sourceRegisterName = _registers.GetRegisterNameGLSL(registerKey);
                    break;
            }

            sourceRegisterName += GetRelativeAddressingName(instruction, srcIndex);
            sourceRegisterName += instruction.GetSourceSwizzleName(srcIndex, destinationLength);
            return ApplyModifier(instruction.GetSourceModifier(srcIndex), sourceRegisterName);
        }

        private string GetSourceName(D3D10Instruction instruction, int srcIndex, int? destinationLength = null)
        {
            string sourceRegisterName;

            var registerKey = instruction.GetParamRegisterKey(srcIndex) as D3D10RegisterKey;
            switch (registerKey.OperandType)
            {
                case OperandType.Immediate32:
                    return GetSourceConstantValue(instruction, srcIndex, destinationLength);
                default:
                    sourceRegisterName = _registers.GetRegisterNameGLSL(registerKey);
                    break;
            }
            sourceRegisterName += instruction.GetSourceSwizzleName(srcIndex, destinationLength);
            return ApplyModifier(instruction.GetOperandModifier(srcIndex), sourceRegisterName);
        }

        private static string GetRelativeAddressingName(Instruction instruction, int srcIndex)
        {
            if (instruction is D3D9Instruction d3D9Instruction && d3D9Instruction.Params.HasRelativeAddressing(srcIndex))
            {
                return "[a0]";
            }
            return string.Empty;
        }

        private string GetSourceConstantValue(D3D9Instruction instruction, int srcIndex, int? destinationLength = null)
        {
            var registerType = instruction.GetParamRegisterType(srcIndex);
            int registerNumber = instruction.GetParamRegisterNumber(srcIndex);
            byte[] swizzle = instruction.GetSourceSwizzleComponents(srcIndex);

            if (destinationLength == null)
            {
                if (instruction.HasDestination)
                {
                    int writeMask = instruction.GetDestinationWriteMask();
                    destinationLength = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if ((writeMask & (1 << i)) != 0)
                        {
                            destinationLength++;
                        }
                    }
                }
                else
                {
                    if (instruction is D3D9Instruction d3D9Instruction
                        && (d3D9Instruction.Opcode == Opcode.If || d3D9Instruction.Opcode == Opcode.IfC))
                    {
                        // TODO
                    }
                    destinationLength = 4;
                }
            }

            switch (registerType)
            {
                case RegisterType.ConstBool:
                    throw new NotImplementedException();
                case RegisterType.ConstInt:
                {
                    var constantInt = _registers.ConstantIntDefinitions.FirstOrDefault(x => x.RegisterIndex == registerNumber);
                    if (constantInt == null)
                    {
                        return null;
                    }

                    uint[] constant = swizzle
                        .Take(destinationLength.Value)
                        .Select(s => constantInt[s]).ToArray();

                    switch (instruction.GetSourceModifier(srcIndex))
                    {
                        case SourceModifier.None:
                            break;
                        case SourceModifier.Negate:
                        case SourceModifier.Abs:
                        case SourceModifier.AbsAndNegate:
                            throw new NotImplementedException();
                        default:
                            throw new NotImplementedException();
                    }

                    if (constant.Skip(1).All(c => constant[0] == c))
                    {
                        return constant[0].ToString(_culture);
                    }
                    string size = constant.Length == 1 ? "" : constant.Length.ToString();
                    return $"int{size}({string.Join(", ", constant)})";
                }
                case RegisterType.Const:
                case RegisterType.Const2:
                case RegisterType.Const3:
                case RegisterType.Const4:
                {
                    var constantRegister = _registers.ConstantDefinitions.FirstOrDefault(x => x.RegisterIndex == registerNumber);
                    if (constantRegister == null)
                    {
                        return null;
                    }

                    float[] constant = swizzle
                        .Take(destinationLength.Value)
                        .Select(s => constantRegister[s]).ToArray();

                    switch (instruction.GetSourceModifier(srcIndex))
                    {
                        case SourceModifier.None:
                            break;
                        case SourceModifier.Negate:
                            for (int i = 0; i < constant.Length; i++)
                            {
                                constant[i] = -constant[i];
                            }
                            break;
                        case SourceModifier.Abs:
                            for (int i = 0; i < constant.Length; i++)
                            {
                                constant[i] = Math.Abs(constant[i]);
                            }
                            break;
                        case SourceModifier.AbsAndNegate:
                            for (int i = 0; i < constant.Length; i++)
                            {
                                constant[i] = -Math.Abs(constant[i]);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    if (constant.Skip(1).All(c => constant[0] == c))
                    {
                        return ConstantFormatter.Format(constant[0]);
                    }
                    if (constant.Length == 1)
                        return ConstantFormatter.Format(constant[0]);
                    return $"vec{constant.Length}({string.Join(", ", constant.Select(c => ConstantFormatter.Format(c)))})";
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private static string GetSourceConstantValue(D3D10Instruction instruction, int srcIndex, int? destinationLength = null)
        {
            D3D10RegisterKey registerKey = instruction.GetParamRegisterKey(srcIndex) as D3D10RegisterKey;
            byte[] swizzle = instruction.GetSourceSwizzleComponents(srcIndex);

            if (destinationLength == null)
            {
                if (instruction.HasDestination)
                {
                    int writeMask = instruction.GetDestinationWriteMask();
                    destinationLength = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if ((writeMask & (1 << i)) != 0)
                        {
                            destinationLength++;
                        }
                    }
                }
                else
                {
                    destinationLength = 4;
                }
            }

            if (registerKey.ImmediateSingle.Length == 1)
            {
                return ConstantFormatter.Format(registerKey.ImmediateSingle[0]);
            }
            string[] constant = swizzle
                            .Take(destinationLength.Value)
                            .Select(s => registerKey.ImmediateSingle[s])
                            .Select(ConstantFormatter.Format)
                            .ToArray();
            return $"vec{destinationLength.Value}(" + string.Join(", ", constant) + ")";
        }

        private static string ApplyModifier(SourceModifier modifier, string value)
        {
            return modifier switch
            {
                SourceModifier.None => value,
                SourceModifier.Negate => $"-{value}",
                // best-effort translations / no-ops for modifiers without direct GLSL equivalent
                SourceModifier.Bias => value,
                SourceModifier.BiasAndNegate => $"-{value}",
                SourceModifier.Sign => value,
                SourceModifier.SignAndNegate => $"-{value}",
                SourceModifier.Complement => throw new NotImplementedException(),
                SourceModifier.X2 => $"(2.0 * {value})",
                SourceModifier.X2AndNegate => $"(-2.0 * {value})",
                SourceModifier.DivideByZ => value,
                SourceModifier.DivideByW => value,
                SourceModifier.Abs => $"abs({value})",
                SourceModifier.AbsAndNegate => $"-abs({value})",
                SourceModifier.Not => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        private static string ApplyModifier(D3D10OperandModifier modifier, string value)
        {
            if (modifier.HasFlag(D3D10OperandModifier.Abs))
            {
                value = $"abs({value})";
            }
            if (modifier.HasFlag(D3D10OperandModifier.Neg))
            {
                value = $"-({value})";
            }
            return value;
        }
    }
}