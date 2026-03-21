package com.reactiveuitk.uitkx

import com.intellij.lang.Language
import com.intellij.openapi.fileTypes.LanguageFileType
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.platform.lsp.api.LspServerSupportProvider
import com.intellij.platform.lsp.api.LspServerSupportProvider.LspServerStarter
import com.intellij.platform.lsp.api.ProjectWideLspServerDescriptor
import com.jetbrains.rider.util.idea.getService
import java.io.File
import java.nio.file.Paths
import javax.swing.Icon

// ── Language ────────────────────────────────────────────────────────────────

object UitkxLanguage : Language("UITKX") {
    @JvmField val INSTANCE: UitkxLanguage = UitkxLanguage
    private fun readResolve(): Any = INSTANCE
}

// ── File type ────────────────────────────────────────────────────────────────

class UitkxFileType private constructor() : LanguageFileType(UitkxLanguage) {

    companion object {
        @JvmField val INSTANCE = UitkxFileType()
    }

    override fun getName()             = "UITKX"
    override fun getDescription()      = "UITKX ReactiveUIToolKit template"
    override fun getDefaultExtension() = "uitkx"
    override fun getIcon(): Icon?      = null   // TODO: supply icon from resources
}

// ── LSP server support ──────────────────────────────────────────────────────

/**
 * Activates the UITKX LSP server for any open .uitkx file.
 */
class UitkxLspServerSupportProvider : LspServerSupportProvider {

    override fun fileOpened(
        project:     Project,
        file:        VirtualFile,
        serverStarter: LspServerStarter,
    ) {
        if (file.extension?.lowercase() == "uitkx") {
            serverStarter.ensureServerStarted(UitkxLspServerDescriptor(project))
        }
    }
}

// ── LSP server descriptor ────────────────────────────────────────────────────

class UitkxLspServerDescriptor(project: Project)
    : ProjectWideLspServerDescriptor(project, "UITKX")
{
    override fun isSupportedFile(file: VirtualFile): Boolean =
        file.extension?.lowercase() == "uitkx"

    override fun createCommandLine(): com.intellij.execution.configurations.GeneralCommandLine {
        val serverDll = findServerDll()
        return com.intellij.execution.configurations.GeneralCommandLine(
            "dotnet", serverDll.absolutePath
        ).also {
            it.isRedirectErrorStream = false
        }
    }

    private fun findServerDll(): File {
        // Look for the server DLL beside the plugin JAR (bundled in the plugin distribution)
        val pluginDir = File(
            UitkxFileType::class.java.protectionDomain.codeSource.location.toURI()
        ).parentFile

        // Bundled path: <plugin-root>/server/UitkxLanguageServer.dll
        val bundled = pluginDir.resolve("server/UitkxLanguageServer.dll")
        if (bundled.exists()) return bundled

        // Fallback: same directory
        val sibling = pluginDir.resolve("UitkxLanguageServer.dll")
        if (sibling.exists()) return sibling

        throw IllegalStateException(
            "UitkxLanguageServer.dll not found. " +
            "Expected it at: ${bundled.absolutePath}. " +
            "Make sure the UITKX plugin was installed correctly."
        )
    }
}
