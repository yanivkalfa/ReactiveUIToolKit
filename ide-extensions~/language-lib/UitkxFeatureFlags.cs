namespace ReactiveUITK.Language
{
    /// <summary>
    /// Build-time feature flags for the import/export grammar (leg 3, plan §12).
    ///
    /// <para><b>Additive-then-flip strategy.</b> Every behavior change of the import/export feature
    /// (path-derived namespaces, per-import scoped injection, path-qualified hook family keys and
    /// component Register ids, <c>export</c>→accessibility, and the strict resolution diagnostics
    /// 2305/2307/2308/2310) is written behind <see cref="StrictImports"/> with the flag DEFAULT
    /// OFF. With the flag off the emitters produce byte-identical legacy output, so the committed
    /// goldens/preview do not move and every step commits green. The flag flips to <c>true</c> in
    /// the SAME commit that runs the migration codemod over the samples and re-pins the goldens
    /// (plan §12 step 6), so implicit cross-file resolution never exists in a compilable state.</para>
    ///
    /// <para>This is the canonical value in language-lib, read directly by the source generator and
    /// the LSP. <c>Editor/HMR</c> cannot reference language-lib (see <c>AssetPathUtil</c>'s doc); it
    /// keeps a byte-for-byte mirror constant pinned by the emitter parity contract test.</para>
    ///
    /// <para>Declared <c>static readonly</c> rather than <c>const</c> on purpose: a <c>const</c>
    /// would make the flag-on branches unreachable code (CS0162) and skip their type-checking until
    /// the flip; <c>static readonly</c> keeps both branches compiled and verified at all times.</para>
    /// </summary>
    public static class UitkxFeatureFlags
    {
        /// <summary>
        /// When <c>true</c>, imports are the sole cross-file resolution mechanism (strict): namespaces
        /// are path-derived, hook/module injection is per-import only, family keys and Register ids are
        /// path-qualified, <c>export</c> drives accessibility, and the 23xx strict diagnostics fire.
        /// Default <c>false</c> until the migration codemod + goldens re-pin land together (plan §12 step 6).
        /// </summary>
        public static readonly bool StrictImports = false;
    }
}
