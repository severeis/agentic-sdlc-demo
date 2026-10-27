
## This issue was reopened

A previous fix for this issue was merged and deployed to production, and a person sent the issue back: the problem is still there. Their comments are at the end of this message. Previous pull requests for this issue are listed below; read their diffs before changing anything.

Before you fix anything, work out why the previous fix did not work in production even though every test passed. Usually it is one of:

- the root cause was wrong, and the change fixed something else;
- the test checked the wrong thing, so it passed without proving the fix;
- production differs from the build machine (see "Production" in CLAUDE.md): culture, file encoding, IIS configuration, data.

Then write a test that fails on the current code for that reason. If the difference cannot be reproduced in a unit test, add a test that guards against it (for example, one that checks the file or setting that caused it) and say so in the summary.

You cannot see screenshots or images in the comments; rely on the text. If the comments do not say what is still wrong and you cannot work it out from the code and the previous diff, report `blocked` and ask the reporter to describe in words what they see and what they expected.

In summary.md, add a section `### Why the previous fix did not work` before `### Fix`.

Previous pull requests:
