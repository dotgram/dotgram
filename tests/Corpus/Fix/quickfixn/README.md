# The message-level cases of the FIX session test suite, as QuickFIX/n carries them

`14a_BadField.def` to `14h_RepeatedTag.def` are copied byte for byte from the QuickFIX/n
repository, `AcceptanceTest/definitions/server/fix44/`, at its `master` of 2026-09-22. They are
QuickFIX/n's transcription of the message-validation cases of the FIX session-level test
suite: each file is one conversation, an `I` line a message the counterparty sends (with a live
SOH, `<TIME>` for a timestamp, and no BodyLength or CheckSum, which the harness that runs them
supplies), and the `E` line after it the answer the engine under test is expected to give. A
Reject there carries `RefTagID` (371) and `SessionRejectReason` (373), which is what a finding
of this package has to be able to say.

`tests/DotGram.Finance.Tests/QuickFixScenarioTests.cs` runs every exchange of them through this
package, under QuickFIX/n's own `FIX44.xml` (the file beside this directory) loaded over the
package's schema, since the expectations were written against it. The other sixty-three files
of that directory are the session's business, sequence numbers, resends and logons, and are not
here.

Licensed under the QuickFIX Software License 1.0, kept in `LICENSE` beside the files it covers;
this product includes software developed by quickfixengine.org.
