# Research Findings

## Main result

The project answered two separate questions.

### Can an external AI/automation process control Autodesk Inventor deeply enough for useful CAD workflows?

**Yes.**

### Does broad API access automatically make an LLM a reliable autonomous engineering drafter?

**No.**

## Strong results

- deterministic JSON-to-Inventor automation;
- scalable Eyes/Hands model;
- factual geometry extraction from features, sketches and BRep;
- assembly-context part reading;
- source-model integrity discipline;
- useful external reasoning for factual dossiers, contradictions and manufacturing equivalence.

## Negative findings

### Feature taxonomy is not geometry

Real bolt holes in Benchmark #2 were Extrude Cuts, not HoleFeatures.

### Model parameter is not always manufacturing dimension

Frame Generator `B_L` could differ from final BRep geometry after trim/split/cut.

### Assembly tree is not manufacturing hierarchy

Native assembly structure did not directly match four welded manufacturing units.

### More commands do not solve visual judgement

At 219 commands the main remaining issues were still:

- view selection;
- dimension architecture;
- sheet composition;
- readability.

### Reference drawings improve planning, not full automation

Reference-aided planning produced better document structures but did not eliminate human visual review.

## Why the original track stopped

The limiting loop became:

```text
new product class
→ new interpretation/reference study
→ new plan
→ render
→ human critique
→ repeated corrections
```

Adding more Inventor commands no longer addressed the core bottleneck.

## Valuable future uses

- model interrogation;
- drawing/model audit;
- fabrication extraction;
- batch Inventor work;
- supervised CAD execution;
- agent tool-layer research.

## Scope

The experiment does not prove autonomous CAD drafting is impossible. It shows that the investigated approach did not reach reliable human-quality generalization at acceptable interaction cost.

The Autodesk Inventor API was not the primary limiting factor.
