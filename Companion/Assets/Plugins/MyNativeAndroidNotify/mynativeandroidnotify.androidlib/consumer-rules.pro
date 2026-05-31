# Плагин MyNativeAndroidNotify вызывается из C# через JNI по строковому имени класса
# (AndroidJavaClass("com.mynative.androidnotify.AlarmApi")). Прямых ссылок из Java/Kotlin нет,
# поэтому R8/минификация (Player Settings → Minify) считает классы неиспользуемыми и вырезает их
# → во время выполнения java.lang.ClassNotFoundException: com.mynative.androidnotify.AlarmApi.
# Запрещаем R8 удалять/переименовывать классы и члены плагина. consumerProguardFiles в build.gradle
# подключает это правило автоматически в приложение, которое использует эту библиотеку.
-keep class com.mynative.androidnotify.** { *; }
