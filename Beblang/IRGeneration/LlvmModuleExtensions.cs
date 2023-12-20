using System.Text;

namespace Beblang.IRGeneration;

public static class LlvmModuleExtensions
{
    public static LLVMValueRef CreateGlobalString(this LLVMModuleRef module, string str)
    {
        // Append the null terminator to the string
        var nullTerminatedStr = str + "\0";
        var charArray = Encoding.ASCII.GetBytes(nullTerminatedStr);
        
        // Create an array of i8 constants for each character
        var chars = charArray.Select(c => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, c, false)).ToArray();
        var strArray = LLVMValueRef.CreateConstArray(LLVMTypeRef.Int8, chars);
        
        // Create a global array containing the string characters
        var globalStr = module.AddGlobal(LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, (uint)nullTerminatedStr.Length), "");
        globalStr.Initializer = strArray;
        globalStr.Linkage = LLVMLinkage.LLVMPrivateLinkage;
        globalStr.IsGlobalConstant = true;
        globalStr.HasUnnamedAddr = true;;

        // Obtain a pointer to the first element of the array
        var zero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0);
        var strPtr = LLVMValueRef.CreateConstInBoundsGEP2(strArray.TypeOf, globalStr, new [] { zero, zero });

        return strPtr;
    }
}