import org.jetbrains.intellij.platform.gradle.TestFrameworkType

plugins {
    id("java")
    id("org.jetbrains.kotlin.jvm") version "1.9.24"
    id("org.jetbrains.intellij.platform") version "2.0.1"
}

group   = "com.reactiveuitk"
version = providers.gradleProperty("pluginVersion").get()

repositories {
    mavenCentral()
    intellijPlatform {
        defaultRepositories()
    }
}

kotlin {
    jvmToolchain(17)
}

dependencies {
    intellijPlatform {
        // Target: Rider 2024.1+
        rider(providers.gradleProperty("platformVersion").get(), useInstaller = false)
        instrumentationTools()
    }
}

intellijPlatform {
    pluginConfiguration {
        id          = providers.gradleProperty("pluginId").get()
        name        = providers.gradleProperty("pluginName").get()
        version     = providers.gradleProperty("pluginVersion").get()
        description = providers.gradleProperty("pluginDescription").get()

        ideaVersion {
            sinceBuild = providers.gradleProperty("pluginSinceBuild").get()
            untilBuild = provider { null }   // open-ended — survives minor bumps
        }

        vendor {
            name = "ReactiveUITK"
            url  = "https://github.com/your-org/ReactiveUIToolKit"
        }
    }

    signing {
        certificateChain = providers.environmentVariable("CERTIFICATE_CHAIN")
        privateKey        = providers.environmentVariable("PRIVATE_KEY")
        password          = providers.environmentVariable("PRIVATE_KEY_PASSWORD")
    }

    publishing {
        token = providers.environmentVariable("PUBLISH_TOKEN")
    }
}
