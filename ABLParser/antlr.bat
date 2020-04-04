@echo on
echo "Generating antlr classes"
set CLASSPATH=%CLASSPATH%;C:\temp\antlr-4.8-complete.jar
call java -Xmx500M org.antlr.v4.Tool -Dlanguage=CSharp Antlr/Grammar/PreprocessorParser.g4 -package ABLParser.Prorefactor.Proparser.Antlr -visitor -lib Antlr/Grammar -o Antlr/Classes
call java -Xmx500M org.antlr.v4.Tool -Dlanguage=CSharp Antlr/Grammar/Proparse.g4 -package ABLParser.Prorefactor.Proparser.Antlr -visitor -lib Antlr/Grammar -o Antlr/Classes