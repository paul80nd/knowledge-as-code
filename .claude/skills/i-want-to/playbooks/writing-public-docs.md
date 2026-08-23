### Writing the public documentation

**The reader has installed nothing and has nobody here to ask.** That is the whole difference from every other surface
in this repository.

1. **Load `technical-writing`, then `writing-the-docs`.** The gloss rule is the one this voice keeps and the other two
   drop, so expect to define words you are used to using bare.
2. **Check every factual claim against the source** before you write it, including one you are rewording rather than
   inventing. A rewrite drops a fact more easily than it drops a word, and the reader can check nothing.
3. **Open on what the reader gets.** A definition answers a question nobody has asked yet, and a problem statement
   under the project's name reads as what the project hands you.
4. **Ask where each fact belongs.** A flag met while running the tool is documented at `--help` and in the reference. A
   page somebody reads before installing carries what decides them.
5. **Measure the prose alone**, with code blocks and tables excluded. The root `README.md` is the register: under four
   contrasts per thousand words, in sentences averaging fourteen.
6. **Pack the package**, where you touched `PACKAGE.md`. `dotnet pack tooling/kac/kac.csproj` proves it still renders
   as the readme nuget.org receives.
7. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what changed and why, the measured figures before and after, and every claim you checked against the source.
