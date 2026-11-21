"""Guard against malformed or mismatched exact-SHA checkout pins in QA CI."""
import pathlib
import re
import unittest

WORKFLOW = pathlib.Path(__file__).resolve().parents[2] / '.github/workflows/qa-offline.yml'
SHA40 = re.compile(r'^[0-9a-f]{40}$')
REF_LINE = re.compile(r'^\s+ref:\s*(\S+)\s*$', re.MULTILINE)
ASSERTION = re.compile(r'\btest\s+"\$[A-Za-z_][A-Za-z_0-9]*"\s*=\s*"([0-9a-f]+)"')


def validate_pins(source):
    """Return actionable errors for cross-branch checkout pins and assertions."""
    errors = []
    jobs = re.split(r'(?m)^  ([a-z][a-z0-9-]+):\s*$', source)
    for index in range(1, len(jobs), 2):
        name, body = jobs[index], jobs[index + 1]
        refs = REF_LINE.findall(body)
        for ref in refs:
            if not SHA40.fullmatch(ref):
                errors.append(f'{name}: invalid checkout SHA {ref!r} (expected 40 lowercase hex chars)')
        asserted = ASSERTION.findall(body)
        for ref in refs:
            if SHA40.fullmatch(ref) and ref not in asserted:
                errors.append(f'{name}: checkout {ref} lacks matching exact-SHA test assertion')
        for sha in asserted:
            if sha not in refs:
                errors.append(f'{name}: asserted SHA {sha} has no matching checkout ref')
    if not REF_LINE.findall(source):
        errors.append('no checkout refs found; parser or workflow layout changed')
    return errors


class WorkflowPinTests(unittest.TestCase):
    def test_real_workflow(self):
        self.assertEqual([], validate_pins(WORKFLOW.read_text(encoding='utf-8')))

    def test_rejects_52_character_ref(self):
        source = 'jobs:\n  npc:\n    steps:\n      - uses: actions/checkout@v4\n        with:\n          ref: ' + 'a'*52 + '\n'
        self.assertIn('invalid checkout SHA', ' '.join(validate_pins(source)))

    def test_rejects_stale_assertion(self):
        source = 'jobs:\n  npc:\n    steps:\n      - uses: actions/checkout@v4\n        with:\n          ref: ' + 'a'*40 + '\n      - run: test "$actual" = "' + 'b'*40 + '"\n'
        self.assertEqual(2, len(validate_pins(source)))


if __name__ == '__main__':
    unittest.main()
