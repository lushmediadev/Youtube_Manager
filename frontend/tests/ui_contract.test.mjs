import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";

const root = "D:/Youtube_manager/frontend";
const appJs = fs.readFileSync(path.join(root, "app.js"), "utf8");
const styleCss = fs.readFileSync(path.join(root, "style.css"), "utf8");

test("frontend requests paged item data and summary data", () => {
  assert.match(appJs, /LIST_PAGE_SIZE:\s*160/);
  assert.match(appJs, /BULK_PAGE_SIZE:\s*1000/);
  assert.match(appJs, /itemsSummary:\s*\(params = \{\}\) => apiFetch\('\/items\/summary'/);
  assert.match(appJs, /api\.items\(\{ \.\.\.params, limit: CONFIG\.LIST_PAGE_SIZE, offset: 0 \}\)/);
});

test("virtual list caches pages and renders viewport placeholders", () => {
  assert.match(appJs, /function resetVirtualList/);
  assert.match(appJs, /function commitPageItems/);
  assert.match(appJs, /function loadVirtualPage/);
  assert.match(appJs, /function renderVirtualPlaceholder/);
  assert.match(appJs, /state\.loadedPageOffsets/);
  assert.match(appJs, /state\.loadingPagePromises/);
  assert.match(styleCss, /\.virtual-row-placeholder/);
});

test("bulk operations fetch the complete scope before export or refresh", () => {
  assert.match(appJs, /async function loadAllItemsForScope/);
  assert.match(appJs, /for \(let offset = CONFIG\.BULK_PAGE_SIZE; offset < total; offset \+= CONFIG\.BULK_PAGE_SIZE\)/);
  assert.match(appJs, /refreshItems\(await loadAllItemsForScope\(\)\)/);
  assert.match(appJs, /exportTxt\(await loadAllItemsForScope\(\), groupLabel\(state\.activeGroup\)\)/);
});

test("excel export downloads a real xlsx blob", () => {
  assert.match(appJs, /async function apiDownload/);
  assert.match(appJs, /exportExcel:\s*\(itemIds\) => apiDownload\('\/items\/export'/);
  assert.match(appJs, /JSON\.stringify\(\{ format: 'xlsx', item_ids: itemIds \}\)/);
  assert.match(appJs, /downloadBlobFile\(exportFileName\(scopeLabel, 'xlsx'\), blob\)/);
  assert.doesNotMatch(appJs, /application\/vnd\.ms-excel/);
});

test("group counts come from backend summary instead of loaded row cache", () => {
  assert.match(appJs, /function groupCount/);
  assert.match(appJs, /state\.itemSummary\?\.all_total/);
  assert.match(appJs, /state\.itemSummary\?\.groups/);
  assert.doesNotMatch(appJs, /\$\{state\.items\.length\}/);
});

test("drag reorder saves row order after loading the active scope", () => {
  assert.match(appJs, /async function reorderRows/);
  assert.match(appJs, /const currentScope = await loadAllItemsForScope\(\)/);
  assert.match(appJs, /const allOwnerItems = await loadAllItemsForScope\(paramsForGroup\(ALL_GROUP_ID\)\)/);
  assert.match(appJs, /await saveRowOrder\(\)/);
  assert.match(appJs, /await loadItems\(\{ preserveScroll: true \}\)/);
});
