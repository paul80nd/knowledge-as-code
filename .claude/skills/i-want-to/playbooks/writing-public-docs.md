### Writing the public documentation

**The reader has installed nothing and has nobody here to ask.** That is the whole difference from every other surface
in this repository.

1. **Where a page already exists, read it cold before you open anything else.** Write down what a reader knows after the
   first paragraph, after the second, and after the first section. That is the diagnosis, and no measurement below will
   produce it: a page can sit inside every target and still fail to say what the thing is.
2. **Load `technical-writing`, then `writing-the-docs`.** The gloss rule is the one this voice keeps and the other two
   drop, so expect to define words you are used to using bare.
3. **Check every factual claim against the source** before you write it, including one you are rewording rather than
   inventing. A rewrite drops a fact more easily than it drops a word, and the reader can check nothing.
4. **Open on what the reader gets.** A definition answers a question nobody has asked yet, and a problem statement under
   the project's name reads as what the project hands you.
5. **Ask where each fact belongs.** A flag met while running the tool is documented at `--help` and in the reference. A
   page somebody reads before installing carries what decides them.
6. **Measure the prose alone**, with code blocks and tables excluded, before and after. Under four contrasts per
   thousand words, in sentences averaging fourteen. Those are the numbers, not a file to compare against, because the
   file you are editing may be the one the register was read from. **The measurement is a prompt to look, never the
   diagnosis.** A page inside both targets can still open on abstract nouns and leave a reader unable to say what it is.
7. **Pack the package**, where you touched `PACKAGE.md`. `dotnet pack tooling/kac/kac.csproj` proves it still renders as
   the readme nuget.org receives.
8. Run **[opening-a-pull-request](opening-a-pull-request.md)**.

**Reply:** what changed and why, the measured figures before and after, and every claim you checked against the source.
