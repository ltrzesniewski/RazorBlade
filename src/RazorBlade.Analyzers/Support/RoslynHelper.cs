using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RazorBlade.Analyzers.Support;

internal static class RoslynHelper
{
    public static bool AreParameterTypesEqual(ImmutableArray<IParameterSymbol> paramsA, ImmutableArray<IParameterSymbol> paramsB)
    {
        // See MemberSignatureComparer.DuplicateSourceComparer

        if (paramsA.Length != paramsB.Length)
            return false;

        for (var i = 0; i < paramsA.Length; ++i)
        {
            var paramA = paramsA[i];
            var paramB = paramsB[i];

            if ((paramA.RefKind == RefKind.None) != (paramB.RefKind == RefKind.None))
                return false;

            if (!SymbolEqualityComparer.Default.Equals(paramA.Type, paramB.Type))
                return false;
        }

        return true;
    }
}
