# ESKD Drawing Policy v1.1

This policy controls how the external AI CAD engineer reasons about machine-part
working drawings. It does not define Runtime behavior.

Runtime remains:

```text
Eyes = factual observation
Hands = one explicit Inventor action
```

The external LLM remains responsible for engineering interpretation, drawing
strategy, feature coverage, ESKD/GOST reasoning, global layout composition, and
final quality judgment.

## 1. Purpose and authority

This document translates ESKD/GOST drawing requirements into an operational
procedure for autonomous drawing creation in AI_CAD_ENGINEER.

Authority hierarchy:

```text
GOST/ESKD requirement
-> project engineering policy
-> model-specific engineering decision
-> Runtime Eyes/Hands execution
```

Runtime must never contain these engineering rules as hidden heuristics. If a
rule requires judgment, the external LLM applies it using factual Runtime Eyes
and atomic Runtime Hands.

Research basis reviewed:

| Standard | Status / relevance | Policy use |
|---|---|---|
| ГОСТ Р 2.109-2023 | Current Russian standard for basic drawing requirements for working design documentation. | Drawing purpose, content, formats, title block references, required manufacturing data. |
| ГОСТ 2.305-2008 | Current standard for views, sections, cuts, and detail elements. | View selection, section/detail decisions, minimum sufficient images. |
| ГОСТ 2.307-2011 | Current standard for dimensions and limit deviations. | Dimension completeness, dimension grouping, non-duplication, layout rules. |
| ГОСТ Р 2.104-2023 | Current Russian standard for title blocks. | Title block and record-part expectations. |
| ГОСТ 2.301-68 | Current standard for drawing sheet formats. | Sheet format selection and sheet bounds. |
| ГОСТ 2.302-68 | Current standard for scales. | Scale choice and indication. |
| ГОСТ 2.303-68 | Current standard for line types and uses. | Lines, centerlines, hidden lines, cutting/section conventions. |
| ГОСТ 2.304-81 | Current standard for drawing fonts. | Readability and text consistency. |
| ГОСТ 2.306-68 | Current standard for material graphic designations in sections. | Section hatching/material graphics. |
| ГОСТ Р 2.316-2023 | Current Russian replacement for ГОСТ 2.316-2008 in RF. | Text, technical requirements, tables, notes, and when text is appropriate. |
| ГОСТ 2.311-68 | Thread representation and thread designation. | Thread callouts and representation. |
| ГОСТ 2.318-81 | Simplified hole dimensioning. | Hole callout policy where simplification is justified. |
| ГОСТ 2.309-73 | Surface finish designation. | Surface finish only when factual requirements exist. |

Important classification:

- [NORMATIVE] means a direct requirement or permission summarized from a cited
  standard.
- [PRACTICE] means accepted drafting practice derived from standards and
  manufacturing readability.
- [PROJECT POLICY] means an AI_CAD_ENGINEER workflow rule created to prevent
  observed failures.

## 2. Drawing completion definition

The drawing is not a picture of the model. It is a manufacturing information
system.

ГОСТ Р 2.109-2023 section 4.1 states that the drawing is intended for
manufacture, assembly, installation, and inspection. [NORMATIVE]

Completion has three separate gates:

### GEOMETRY COMPLETE

All manufacturing-significant geometric regions of the part are represented by
views, sections, details, standard symbols, or notes where text is genuinely the
correct representation. [PROJECT POLICY]

Geometry complete does not require a perfect layout, but no important feature may
be invisible or omitted. [PROJECT POLICY]

### MANUFACTURING COMPLETE

The drawing contains enough dimensions, limits, symbols, callouts, and required
specifications for a competent manufacturer to make and inspect the part without
opening the 3D model. [PROJECT POLICY]

ГОСТ Р 2.109-2023 section 5.10 requires drawings to indicate dimensions, limit
deviations, surface roughness requirements, and other data according to the
applicable ESKD standards. [NORMATIVE]

Manufacturing complete must not be declared if a visible feature lacks required
size, position, depth, quantity, angle, radius, chamfer, thread, or other
manufacturing data. [PROJECT POLICY]

### LAYOUT COMPLETE

The complete information set is arranged into a readable, professional drafting
composition. [PROJECT POLICY]

Layout complete is not equivalent to collision-free. A drawing can have no
detected bounding-box collisions and still be poorly composed. [PROJECT POLICY]

## 3. Mandatory model-understanding phase

Before creating any drawing, the AI must inspect the model and produce a feature
inventory. [PROJECT POLICY]

Minimum model-understanding actions:

- inspect model feature tree;
- inspect parameters and named dimensions;
- inspect hole and thread features;
- inspect surface bodies, faces, and edges where features are ambiguous;
- inspect sketches, sketch geometry, dimensions, and constraints when needed;
- inspect work features and axes;
- inspect model previews from multiple useful orientations;
- compare visual previews with structural facts;
- identify axes, symmetry, datum-like manufacturing references;
- identify holes, threads, recesses, slots, flats, keyways, pockets, shoulders,
  chamfers, fillets, repeated features, patterns, and internal geometry;
- identify features requiring a section, local section, cross-section, detail, or
  auxiliary view.

Every manufacturing-significant feature receives a unique checklist entry before
drawing mutation. [PROJECT POLICY]

Do not ignore a feature because Inventor labels it generically, for example
`ExtrudeFeature`. The physical geometry controls the drawing task. [PROJECT POLICY]

## 4. Feature coverage matrix

Before creating dimensions, construct a logical matrix:

```text
FEATURE
MODEL FACTS
BEST REPRESENTATION
REQUIRED MANUFACTURING INFORMATION
PLANNED VIEW / SECTION / DETAIL
PLANNED DIMENSIONS / CALLOUTS
STATUS
```

Allowed status values:

- `UNREPRESENTED`
- `VISIBLE`
- `PARTIALLY_DEFINED`
- `FULLY_DEFINED`

`VISIBLE` must never be treated as `FULLY_DEFINED`. [PROJECT POLICY]

`FULLY_DEFINED` means the future drawing, without the model, communicates the
feature's manufacturing geometry and requirements unambiguously. [PROJECT POLICY]

### Feature requirement checklist

Every manufacturing-significant feature must be decomposed into atomic
requirements before it can be marked `FULLY_DEFINED`. [PROJECT POLICY]

MODEL KNOWLEDGE IS NOT DRAWING COVERAGE. For every requirement row, the final
audit must identify drawing-side evidence visible or readable from the drawing
itself: a dimension, native callout, section geometry, visible geometry, hidden
geometry, symbol, table, or a technical requirement where text is legitimately
the correct representation. [PROJECT POLICY]

If the model contains a fact but the final drawing preview/readback does not
communicate it to a reader without the 3D model, that requirement is
`MISSING` or `PARTIAL`. [PROJECT POLICY]

Use this requirement checklist as the minimum decomposition:

```text
FEATURE
REQUIREMENT
MODEL FACT
DRAWING-SIDE EVIDENCE
STATUS
```

Requirement status values:

- `COVERED`
- `PARTIAL`
- `MISSING`

Feature status is derived from requirement rows:

- `FULLY_DEFINED` only when every required row is `COVERED`.
- `PARTIALLY_DEFINED` when at least one required row is `PARTIAL` or `MISSING`
  but the feature is at least represented.
- `MISSING` when the feature has no usable drawing representation.

Hole requirement rows include:

- count;
- diameter;
- position;
- depth or through condition;
- axis or orientation where relevant;
- countersink/counterbore geometry where present;
- internal end or drill-point geometry where manufacturing-relevant;
- tolerance if factually provided by source or specification.

Thread requirement rows include:

- designation;
- internal/external state;
- class/tolerance where provided;
- length or depth;
- position;
- termination where relevant.

Recess, slot, flat, and cut-feature requirement rows include:

- length;
- width;
- depth;
- position;
- orientation;
- end geometry;
- chamfers, radii, or transitions.

Repeated-feature requirement rows include:

- quantity;
- feature size;
- feature condition, such as through/blind/threaded;
- complete pattern or individual location definition;
- orientation or plane where relevant.

## 5. View-selection policy

ГОСТ 2.305-2008 section 4.9 requires the number of images to be minimal but
sufficient to provide a complete understanding of the item using established
symbols, signs, and inscriptions. [NORMATIVE]

ГОСТ 2.305-2008 section 4.3 establishes the front-plane image as the main image
and requires the object to be oriented so that it gives the most complete idea of
form and dimensions. [NORMATIVE]

Operational rules:

- Choose the main view that best communicates the part's characteristic form,
  manufacturing axis, and dimensioning basis. [PRACTICE]
- Use the minimum number of views necessary, but never fewer than needed for
  unambiguous manufacturing. [NORMATIVE]
- Do not add views merely because empty sheet space exists. [PROJECT POLICY]
- Do not omit a view when another representation is needed to dimension a feature
  clearly. [PROJECT POLICY]
- Prefer projected views for shape and location when orthographic relation is
  sufficient. [PRACTICE]
- Use sections when internal or hidden geometry would otherwise require unclear
  hidden-line dimensioning. [PRACTICE]
- Use detail views when the feature is too small or crowded to dimension at base
  scale. [PRACTICE]
- Use auxiliary views when a feature lies on an inclined plane and a true shape
  or true size is needed. [PRACTICE]

## 6. Dimensioning policy

ГОСТ 2.307-2011 section 4.1 establishes that dimensions on the graphic document
are the basis for determining the size of the product and its elements, except
for special cases covered by other standards. [NORMATIVE]

ГОСТ 2.307-2011 section 4.2 requires the total number of dimensions to be minimal
but sufficient for manufacture and inspection. [NORMATIVE]

ГОСТ 2.307-2011 section 4.6 prohibits repeating dimensions of the same element on
different images, in technical requirements, title block, and specification,
except defined reference-size cases. [NORMATIVE]

ГОСТ 2.307-2011 section 4.7 states that linear dimensions in graphic documents
are in millimeters without the unit symbol; dimensions in technical requirements
and explanatory notes require units. [NORMATIVE]

ГОСТ 2.307-2011 section 4.13 prohibits closed dimension chains except where one
dimension is reference. [NORMATIVE]

ГОСТ 2.307-2011 section 5.32 recommends grouping dimensions for the same
constructive element in one place, on the image where that element's geometry is
shown most fully. [NORMATIVE]

Operational dimension rules:

- Overall dimensions define envelope, length, width, height, and key diameters
  needed to bound the part. [PRACTICE]
- Functional and manufacturing dimensions define each feature from an explicit
  manufacturing or inspection basis. [PRACTICE]
- Do not derive a required manufacturing dimension only indirectly if a direct
  dimension is needed for manufacture or inspection. [PROJECT POLICY]
- Prefer dimensions on the image where the feature is represented most clearly.
  [NORMATIVE]
- Do not place a dimension merely because Runtime can create it there. [PROJECT POLICY]
- Avoid unnecessary dimension duplication. [NORMATIVE]
- Avoid closed chains unless the reference dimension rule is deliberately used.
  [NORMATIVE]
- Group related dimensions visually and logically. [NORMATIVE]
- Dimension diameter with the diameter symbol and radius with `R`, as required by
  ГОСТ 2.307-2011 sections 5.33 and 5.38. [NORMATIVE]
- Quantity, size, location, depth, and angle are separate information
  requirements. A callout may combine them only if the standard notation and
  readability remain clear. [PROJECT POLICY]

Dimension layout requirements:

- ГОСТ 2.307-2011 section 5.9 prefers dimension lines outside the image contour.
  [NORMATIVE]
- Section 5.11 sets minimum spacing between parallel dimension lines and between
  a dimension line and the contour, with final choice depending on drawing
  density. [NORMATIVE]
- Section 5.12 requires avoiding intersections of dimension and extension lines.
  [NORMATIVE]
- Section 5.13 prohibits using contour, axis, center, or extension lines as
  dimension lines. [NORMATIVE]
- Section 5.31 prohibits dimension numbers and limit deviations being separated
  or crossed by image lines. [NORMATIVE]

## 7. Native annotation vs prose policy

ГОСТ Р 2.316-2023 section 4.3 says text is included when the information,
instruction, or explanation cannot be expressed graphically or it is not
expedient to express it graphically. [NORMATIVE]

Therefore:

GENERAL PROSE IS NOT A SUBSTITUTE FOR GRAPHICAL DIMENSIONING OR STANDARD NATIVE
CALLOUTS WHEN A PROPER ENGINEERING REPRESENTATION IS AVAILABLE. [PROJECT POLICY]

Bad:

```text
Глубина выборки 5 мм
```

when a graphical depth dimension or section/detail dimension can define the
feature. [PROJECT POLICY]

Bad:

```text
Длина выборки 25 мм
```

when a normal linear dimension on the appropriate view is available. [PROJECT POLICY]

Bad:

```text
Фаска на концах выборки 2 x 45 градусов
```

when a standard chamfer dimension/callout can be placed on the feature. [PROJECT POLICY]

Use general notes only for genuinely textual or global information, such as:

- common technical requirements;
- material, heat treatment, coating, surface finish, or inspection requirements
  when factually available and not better expressed at individual features;
- references to applicable documents;
- brief clarifying statements that cannot be expressed clearly through symbols or
  dimensions.

ГОСТ Р 2.316-2023 sections 5.1 to 5.4 require inscriptions to be concise, clear,
placed relative to the relevant image, and used for short image-related
information. [NORMATIVE]

## 8. Holes and repeated-feature policy

For each hole determine:

- quantity;
- diameter;
- through or blind condition;
- depth where blind or stepped;
- counterbore/countersink diameter and depth/angle where present;
- thread if present;
- center position;
- pattern definition if repeated;
- view, section, or callout that communicates the hole clearly.

ГОСТ 2.307-2011 sections 5.45 to 5.48 cover repeated elements: repeated identical
elements are typically dimensioned once with quantity, identical holes require
full quantity, and evenly spaced repeated elements may be specified by spacing
and extreme distance rather than a full chain. [NORMATIVE]

ГОСТ 2.318-81 sections 2 and 3 permit simplified hole dimensioning in defined
cases, including very small visible holes, absence of an axial section, or when
general dimensioning would make the drawing hard to read, and indicate the hole
size on a leader shelf from the hole axis. [NORMATIVE]

Project rule:

- A note saying `5 отв. Ø2` does not define the hole pattern by itself.
  Positions must be dimensioned by baseline, ordinate, chain, coordinates, hole
  table, or another clear standard method. [PROJECT POLICY]
- A note, view, or model fact saying a hole exists does not automatically cover
  depth or through condition. The drawing-side evidence must visibly communicate
  through, blind depth, or other end condition. [PROJECT POLICY]
- For repeated through holes, the final drawing must communicate quantity,
  diameter, through condition, and all locations or a complete pattern
  definition. If the visible/native note shows only `Ø2`, quantity and through
  condition remain not covered even when the model Eye proves them. [PROJECT POLICY]
- A native Inventor `HoleThreadNote` is sufficient only after readback and/or
  preview confirms that the visible/formatted output contains every required
  semantic element for that hole. [PROJECT POLICY]
- If a native note omits a required element, try safe existing formatting
  capabilities before accepting the deficiency. If it still cannot be expressed,
  record the exact limitation and keep the feature `PARTIALLY_DEFINED`.
  [PROJECT POLICY]
- If a hole table is used, verify that hole tags, table rows, and view geometry
  together define the feature. [PROJECT POLICY]
- If the holes are not equally spaced, do not imply equal spacing. [PROJECT POLICY]

## 9. Thread policy

ГОСТ 2.311-68 establishes rules for thread representation and designation on
drawings. [NORMATIVE]

For every thread determine:

- external or internal;
- thread designation, pitch, tolerance/class where available;
- thread length;
- start/end location;
- blind/through context for internal thread;
- associated pilot hole/countersink/chamfer where relevant;
- view or section that shows the threaded feature clearly.

Use native Inventor hole/thread notes when they produce correct standard
designation. Do not replace a native thread callout with prose unless no native
or graphical representation is available. [PROJECT POLICY]

## 10. Recess / slot / flat / cut-feature policy

This section directly addresses the `Выдавливание3` failure.

For any nontrivial cut, recess, slot, flat, pocket, keyway, or one-sided
longitudinal feature, determine:

- axial or linear extent;
- transverse width or chord;
- depth;
- position from manufacturing datum or relevant end/shoulder;
- orientation or side around the part;
- end geometry;
- radii, chamfers, or transition geometry;
- relation to surrounding cylindrical, planar, or shoulder geometry;
- whether the feature is open-ended, blind, through, tangent, or partial-depth.

A feature is not `FULLY_DEFINED` because it is visible in model preview or on a
drawing view. It is `FULLY_DEFINED` only when the drawing communicates size,
position, depth, orientation, and end/transition geometry. [PROJECT POLICY]

For `Выдавливание3`-type recesses:

- use the view where the recess outline is visible to show length and width where
  possible;
- use a section, local section, cross-section, or detail when depth or side
  orientation is not clear in the base view;
- dimension end chamfers/radii graphically where possible;
- keep recess identifier, detail view, dimensions, and any necessary note as one
  visual group;
- do not define the recess by a paragraph if standard dimensions can be placed.

## 11. Section/detail decision policy

ГОСТ 2.305-2008 distinguishes views, sections, and cuts and defines their use for
revealing shape and hidden surfaces. [NORMATIVE]

Consider a section when:

- internal geometry must be manufactured or inspected;
- hidden lines would be crowded or ambiguous;
- depth or wall thickness cannot be dimensioned clearly from an exterior view;
- a hole, recess, or undercut needs its internal profile shown.

### Internal-geometry representation gate

For blind holes, counterbores, countersinks, internal shoulders, drill-point or
conical bottoms, and internal recesses, ask:

```text
Can the internal profile be unambiguously understood from the selected views?
```

If the answer is no, consider a section, local section, broken-out section, or
hidden-line view before accepting the feature as represented. [PROJECT POLICY]

This is not a claim that every blind hole normatively requires a section. It is
an AI_CAD_ENGINEER coverage gate to prevent the agent from replacing internal
profile reasoning with an end-view note. [PROJECT POLICY]

During drawing planning, every internal feature must include this row set:

```text
FEATURE
VISIBLE IN NORMAL VIEW?
HIDDEN-LINE REPRESENTATION SUFFICIENT?
SECTION NEEDED?
PLANNED REPRESENTATION
```

If a blind hole has a drill-point or conical bottom that affects manufacture or
inspection, the requirement checklist must include that internal end geometry.
It may be covered by a clear section/hidden-line representation, a standard
native callout that visibly communicates the end condition, or a legitimate
technical requirement when text is the correct representation. [PROJECT POLICY]

Consider a local section when:

- only one region needs internal clarification;
- a full section would overload the sheet;
- the feature can be isolated without losing context.

Consider a cross-section when:

- the true profile of a shaft, slot, recess, groove, or wall is needed;
- the feature is defined by a transverse shape.

Consider a detail view when:

- a small region is too small or crowded at base scale;
- existing dimensions cannot be read clearly at base scale;
- local features need grouped, enlarged annotation.

A detail view must not be used merely to make something larger if the feature
still cannot be unambiguously dimensioned. [PROJECT POLICY]

## 12. Content-first workflow

Autonomous drawing workflow:

### PHASE A - MODEL UNDERSTANDING

Read model structure and previews. Identify physical features and manufacturing
meaning before drawing mutation.

### PHASE B - FEATURE INVENTORY

Create a checklist entry for every manufacturing-significant feature.

### PHASE C - COVERAGE PLAN

For every feature, define representation and required dimensions/callouts before
creating annotation.

### PHASE D - VIEW CREATION

Create the planned main view, projected views, sections, details, and auxiliary
views. Avoid views not justified by the coverage plan.

### PHASE E - CONTENT CREATION

Create all required dimensions, notes, native callouts, center annotations,
tables, and title block data available from facts. Use provisional placement
only as needed to keep working.

### PHASE F - CONTENT FREEZE

Declare content freeze only when no planned manufacturing information remains to
be added.

Before content freeze, every feature must have a requirement checklist with
drawing-side evidence for each required fact. Content freeze is allowed only
when no requirement is `MISSING`, unless the missing item is explicitly recorded
as a Runtime or source-data limitation. If such a limitation remains, the final
result cannot be `COMPLETE`. [PROJECT POLICY]

Do not promote `PARTIAL` requirement rows to `COVERED` because the agent knows
the model fact, because the feature is visible, or because a related note exists.
[PROJECT POLICY]

### PHASE G - GLOBAL LAYOUT

After content freeze, all existing objects may move. Do not freeze earlier
annotations just because they were placed first.

### PHASE H - VISUAL QA

Capture drawing preview and critique the complete composition.

### PHASE I - STRUCTURAL QA

Use `get_drawing_layout_map` and other Eyes to identify exact objects,
coordinates, associations, and readback after moves.

### PHASE J - FINAL FEATURE COVERAGE AUDIT

Return to the feature inventory and mark every feature.

### PHASE K - SAVE / EXPORT

Save/export only after content and layout are acceptable or limitations are
explicitly recorded.

No global aesthetic optimization before content freeze, except minimal placement
needed to continue creating content. [PROJECT POLICY]

## 13. Global layout policy

After content freeze, consider the entire sheet simultaneously.

The objective is not merely:

```text
zero bounding-box collisions
```

The objective is professional drafting composition. [PROJECT POLICY]

Evaluate:

- projection relationships;
- grouping by feature and by view;
- visual hierarchy;
- logical association between annotation and feature;
- dimension zones around each view;
- note zones and table zones;
- whitespace and balance;
- title-block clearance;
- border clearance;
- leader routing;
- crossing minimization;
- readability at final sheet scale.

A collision-free drawing may still be badly composed. [PROJECT POLICY]

Distribute information among available suitable views according to semantic
relevance. Do not overload the main view while leaving another technically
suitable view unused. Dimensions and callouts may be placed on another valid
view if that view represents the same feature more clearly and reduces
ambiguity or crowding. [PROJECT POLICY]

## 14. Dimension layout hierarchy

Use these reasoning rules, not hard-coded sheet coordinates:

- feature-local dimensions closest to the feature;
- intermediate dimensions next outward;
- overall dimensions farthest outward;
- related dimensions grouped in one region;
- dimensions for one feature should not migrate to unrelated empty space;
- avoid dimension text over model geometry;
- avoid dimension text on contour, center, hidden, or hatch lines;
- avoid unnecessary leader crossings;
- preserve recognizable chains, baselines, and ordinate systems;
- avoid alternating sides randomly unless it improves clarity;
- keep dimensions visually attached to the view where the feature is clearest.

These are reasoning rules for the external LLM. Do not encode them as Runtime
automatic layout heuristics. [PROJECT POLICY]

## 15. Visual self-review

Use `capture_drawing_sheet_preview` after meaningful layout stages.

The AI must critique the image as a human drafter would:

- Can each dimension be read immediately?
- What manufacturing facts are actually visible to a reader who has no access to
  the model?
- Which feature does each dimension belong to?
- Are dimensions crossing geometry?
- Are dimension numbers crossed by any lines?
- Are annotations crossing each other?
- Are leaders unnecessarily long?
- Are related items grouped?
- Is any feature visually overloaded?
- Is empty space being used intelligently?
- Does the drawing read in a logical order?
- Are source/detail labels clear and not touching model geometry?
- Are title block and border clear?
- Can the reader tell whether each hole is through or blind?
- Can the reader understand the internal end geometry of blind holes,
  counterbores, countersinks, and drill-point bottoms where relevant?
- Can the reader tell the orientation or side of recesses, flats, slots, and
  one-sided cuts?
- Do native notes visibly contain all required elements, such as quantity,
  diameter, depth/through condition, thread designation, and class?

If visual evidence contradicts structural AABB checks, trust the visual problem
and investigate. [PROJECT POLICY]

## 16. Structural layout review

Use `get_drawing_layout_map` together with preview.

Structural geometry is evidence, not the final aesthetic judge. [PROJECT POLICY]

Use structural facts to identify:

- exact view rectangles;
- dimension text boxes;
- dimension lines and extension lines;
- note bounds;
- leader nodes/segments where available;
- title block and border bounds;
- stable object identities or collection indices;
- actual positions after moves.

AABB intersection alone is insufficient. It does not capture poor grouping,
excessive leader length, ambiguous association, visual imbalance, or text that
looks bad despite barely missing a bounding box. [PROJECT POLICY]

## 17. Final feature coverage audit

Return to the original feature inventory.

For every feature, produce a requirement table:

```text
FEATURE
REQUIREMENT
MODEL FACT
DRAWING EVIDENCE
STATUS
```

Requirement status values:

- `COVERED`
- `PARTIAL`
- `MISSING`

Then derive the feature status from the requirement rows:

- `FULLY_DEFINED`
- `PARTIALLY_DEFINED`
- `MISSING`

Do not use a single summary row such as `Lower axial hole - FULLY_DEFINED`
without checking each required subrequirement. [PROJECT POLICY]

The drawing must not be declared complete while any manufacturing-significant
feature is `MISSING`. [PROJECT POLICY]

`PARTIALLY_DEFINED` must be reported explicitly with the missing information and
the reason it could not be added. [PROJECT POLICY]

A feature is `FULLY_DEFINED` only if every required subrequirement has
drawing-side evidence. Model facts, mental memory, or preview understanding are
not substitutes for drawing-side evidence. [PROJECT POLICY]

## 18. Final ESKD QA checklist

Before final save/export, verify:

- views: main view is characteristic; projected views are justified;
- sections/details: each one serves a defined manufacturing need;
- dimensions: minimum but sufficient, non-duplicated, readable;
- holes: quantity, diameter, condition, depth, and positions defined;
- internal hole geometry: countersinks, counterbores, drill points, blind ends,
  and internal shoulders are represented or legitimately called out;
- threads: designation, length, location, and representation defined;
- chamfers: size/angle or standard note present where required;
- radii/fillets: defined where manufacturing-significant;
- recesses/slots/flats: length, width, depth, position, orientation, ends, and
  transitions defined;
- repeated features: quantity and pattern definition complete;
- repeated features: size, condition, complete location definition, and
  orientation/plane are covered by drawing-side evidence;
- annotations: native callouts used where available; prose limited to proper
  textual requirements;
- native notes: visible/formatted output contains every required semantic
  element; source metadata alone is not enough;
- scale: sheet/title view scale facts are correct; nonstandard view scales are
  indicated;
- frame/title block: present and not obstructed;
- layout: title block, border, views, dimensions, notes, and leaders readable;
- feature coverage: every inventory item audited;
- manufacturing completeness: the drawing can be used without the 3D model.

## 19. Failure behavior

If Runtime lacks a required capability:

- do not fake the drawing with prose unless prose is actually the correct
  engineering representation;
- report the required engineering representation;
- report the available Runtime capability;
- report the exact blocker;
- report the smallest missing Eye/Hand;
- report whether a safe approximation exists;
- mark the drawing `PARTIALLY_COMPLETE` if the feature cannot be fully defined.

Do not silently convert a missing graphical dimension into an explanatory note.
[PROJECT POLICY]

## 20. Project-specific anti-patterns

ANTI-PATTERN: feature visible therefore feature defined.

Correct rule: visible is only `VISIBLE`. It must still be dimensioned or called
out. [PROJECT POLICY]

ANTI-PATTERN: replace dimensions with explanatory prose.

Correct rule: use graphical dimensions or native callouts when available.
[PROJECT POLICY]

ANTI-PATTERN: place every new annotation into currently empty space.

Correct rule: place annotations by feature association and dimension hierarchy.
[PROJECT POLICY]

ANTI-PATTERN: optimize each annotation independently.

Correct rule: defer final layout until content freeze and then recompose the
whole sheet. [PROJECT POLICY]

ANTI-PATTERN: declare success because there are no bounding-box collisions.

Correct rule: visual composition and manufacturing completeness are separate
gates. [PROJECT POLICY]

ANTI-PATTERN: use model preview as substitute for manufacturing definition.

Correct rule: previews inform understanding; the drawing must communicate the
manufacturing definition. [PROJECT POLICY]

ANTI-PATTERN: model fact known therefore drawing fact covered.

Correct rule: every required fact must have drawing-side evidence visible or
readable from the drawing. [PROJECT POLICY]

ANTI-PATTERN: native note exists therefore the hole/thread feature is fully
defined.

Correct rule: verify the visible/formatted native note output contains every
required element; otherwise the missing element remains `PARTIAL` or `MISSING`.
[PROJECT POLICY]

ANTI-PATTERN: summarize a complex feature as `FULLY_DEFINED` without checking
subrequirements.

Correct rule: derive feature status from atomic requirement rows. [PROJECT POLICY]

ANTI-PATTERN: use an end view or leader note to avoid considering internal
profile representation.

Correct rule: blind holes, countersinks, counterbores, drill-point bottoms, and
internal shoulders require an explicit internal-geometry representation decision.
[PROJECT POLICY]

ANTI-PATTERN: move dimensions/notes without readback and visual verification.

Correct rule: after movement, read factual state and inspect preview. [PROJECT POLICY]

ANTI-PATTERN: ignore a feature because its Inventor feature type is generic
`ExtrudeFeature` rather than semantic `SlotFeature`.

Correct rule: classify physical manufacturing geometry from structure plus
visual previews. [PROJECT POLICY]

## Shaft fixture mental validation

For `вал тестовый.ipt`, this policy would have prevented the observed failures:

- Missing `Выдавливание3`: mandatory model-understanding and feature inventory
  require every manufacturing-significant recess/cut to be listed before drawing.
- Recess dimensions absent: the recess policy requires length, width, depth,
  position, orientation, and end/transition geometry.
- Prose replacing recess dimensions: the native annotation vs prose policy
  forbids prose as a substitute when graphical dimensions are available.
- Weak detail representation: the section/detail policy requires the view or
  detail to expose the dimensions, not merely enlarge the feature.
- Dimensions crossing geometry: the dimensioning policy cites ГОСТ 2.307-2011
  layout rules and requires visual self-review.
- Greedy annotation placement: the content-first workflow freezes content before
  global layout.
- Layout accepted because collisions were zero: global layout policy explicitly
  rejects collision-free as sufficient.
- Lower blind axial hole internal geometry omitted: the internal-geometry gate
  requires the blind bore, countersink, and drill-point/conical end to be checked
  as drawing requirements, not just known from the model.
- Repeated Ø2 holes accepted without visible through condition: the hole
  checklist separates count, diameter, location, and through/blind condition, so
  the feature remains `PARTIALLY_DEFINED` until the drawing communicates through.
- Repeated-hole orientation/location underchecked: the repeated-feature checklist
  requires feature condition, complete location definition, and orientation or
  plane where relevant.
- Main view overloaded while another suitable view is underused: the global
  layout policy now requires distributing information by semantic relevance
  rather than preserving first placement.

## Areas requiring engineering judgment

The following cannot be reduced to a fixed rule without violating the project
architecture:

- choosing exact main-view orientation for unusual parts;
- deciding when a local section is clearer than a detail view;
- selecting baseline versus chain versus ordinate dimensions;
- deciding when a note is genuinely more correct than a graphical dimension;
- choosing which dimensions are functional versus purely manufacturing;
- determining whether a safe approximation is acceptable when a Runtime Hand is
  missing;
- deciding final sheet composition after preview inspection.

These remain external LLM responsibilities.

## Source links

- ГОСТ Р 2.109-2023, Росстандарт: https://protect.gost.ru/gost/details/b88d9f1e-79f8-43ec-ba93-23fb5696d1bb
- ГОСТ Р 2.109-2023, accessible text excerpt: https://docs.cntd.ru/document/1303625491
- ГОСТ 2.305-2008, Росстандарт: https://protect.gost.ru/gost/details/cbeccb9a-67df-45fa-9c67-c391a43bbbc9
- ГОСТ 2.305-2008, accessible text excerpt: https://meganorm.ru/Data2/1/4293830/4293830650.htm
- ГОСТ 2.307-2011, Росстандарт: https://protect.gost.ru/gost/details/62de9da6-e1a7-4023-955e-7dd9777eaab6
- ГОСТ 2.307-2011, accessible text excerpt: https://base.garant.ru/70217464/
- ГОСТ Р 2.104-2023, Росстандарт: https://protect.gost.ru/gost/details/69d422a2-3c0e-4561-a521-0c99f9cf4417
- ГОСТ 2.301-68, Росстандарт: https://protect.gost.ru/gost/details/aa681f87-ff49-4115-ad25-169e15d2af06
- ГОСТ 2.302-68, Росстандарт: https://protect.gost.ru/gost/details/b8e1f82a-8ad2-4133-8c49-2570711b7e22
- ГОСТ 2.303-68, Росстандарт: https://protect.gost.ru/gost/details/09645eb7-ee23-467a-a319-25b3112a7227
- ГОСТ 2.304-81, Росстандарт: https://protect.gost.ru/gost/details/2acb3258-c28c-4e4a-b7ac-208043f2d95e
- ГОСТ 2.306-68, Росстандарт: https://protect.gost.ru/gost/details/150d6790-3ea9-4022-b6bc-3282b62f230d
- ГОСТ Р 2.316-2023, Росстандарт: https://protect.gost.ru/gost/details/25054f20-f35a-45ff-8d1f-1c198c35ea06
- ГОСТ Р 2.316-2023, accessible text excerpt: https://base.garant.ru/408164557/
- ГОСТ 2.311-68, Росстандарт: https://protect.gost.ru/gost/details/75b396ab-4b3e-4305-a871-9809188c6bc4
- ГОСТ 2.318-81, Росстандарт: https://protect.gost.ru/gost/details/86bda19d-e06d-4b74-b175-5541c3c7515b
- ГОСТ 2.309-73, accessible text excerpt: https://base.garant.ru/3924198/
