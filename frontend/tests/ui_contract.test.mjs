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

test("bulk export/delete operations fetch the complete scope only when needed", () => {
  assert.match(appJs, /async function loadAllItemsForScope/);
  assert.match(appJs, /for \(let offset = CONFIG\.BULK_PAGE_SIZE; offset < total; offset \+= CONFIG\.BULK_PAGE_SIZE\)/);
  assert.match(appJs, /exportTxt\(await loadAllItemsForScope\(\), groupLabel\(state\.activeGroup\)\)/);
});

test("large refresh actions run by scope without downloading full rows first", () => {
  assert.match(appJs, /crawlScope:\s*\(params = \{\}\) => apiFetch\('\/crawl\/scope'/);
  assert.match(appJs, /async function refreshScope/);
  assert.match(appJs, /return refreshScope\(paramsForGroup\(contextGroup\), groupLabel\(contextGroup\)\)/);
  assert.doesNotMatch(appJs, /refresh-current'\) return refreshItems\(await loadAllItemsForScope\(\)\)/);
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

test("drag reorder saves row order with lightweight id scopes", () => {
  assert.match(appJs, /async function reorderRows/);
  assert.match(appJs, /async function loadAllItemIdsForScope/);
  assert.match(appJs, /api\.itemIds\(params\)/);
  assert.match(appJs, /applyOptimisticRowReorder\(dragKeys, targetKey, placement\)/);
  assert.match(appJs, /await saveRowOrder\(\)/);
  assert.match(appJs, /loadItems\(\{ preserveScroll: true \}\)\.catch/);
});

test("list navigation renders pending state before network responses", () => {
  assert.match(appJs, /function showListPending/);
  assert.match(appJs, /function loadItemsInBackground/);
  assert.match(appJs, /if \(!options\.skipQueue\) queueVirtualPagesForRange\(start, end\)/);
  assert.match(appJs, /showInstantListOrPending\(getBackendListParams\(\)\);\s*loadItemsInBackground\(\);/);
  assert.match(appJs, /Promise\.all\(\[loadGroups\(\), loadItems\(\)\]\)\.catch/);
});

test("pending navigation does not show page size as real channel total", () => {
  assert.match(appJs, /pendingTotalLabel/);
  assert.match(appJs, /showListPending\(getBackendListParams\(\), \{ total: 0, totalLabel: '\.\.\.', placeholderRows: 10 \}\)/);
  assert.doesNotMatch(appJs, /showListPending\(getBackendListParams\(\), \{ total: CONFIG\.LIST_PAGE_SIZE \}\)/);
});

test("startup hydrates cached list before waiting for API data", () => {
  assert.match(appJs, /function hydrateListCache/);
  assert.match(appJs, /function saveListCache/);
  assert.match(appJs, /if \(!hydrateListCache\(getBackendListParams\(\)\)\) \{\s*showListPending/);
  assert.match(appJs, /saveListCache\(params\)/);
});

test("list scopes preload cached first pages and warm current pages in background", () => {
  assert.match(appJs, /PRELOAD_GROUP_LIMIT:\s*12/);
  assert.match(appJs, /BACKGROUND_WARM_DELAY_MS:\s*180/);
  assert.match(appJs, /async function preloadFirstPageForParams/);
  assert.match(appJs, /function scheduleCurrentScopeWarmup/);
  assert.match(appJs, /loadVirtualPage\(offset, \{ silent: true, render: false \}\)/);
  assert.match(appJs, /function scheduleSiblingGroupPreload/);
  assert.match(appJs, /const groups = \[ALL_GROUP_ID, \.\.\.allGroupsFromItems\(\)\]/);
  assert.match(appJs, /scheduleListPreloads\(\)/);
});

test("delta display never renders zero day denominators", () => {
  assert.match(appJs, /function renderDelta/);
  assert.match(appJs, /Math\.max\(1, Number\(item\.delta_days\) \|\| 1\)/);
});

test("small group navigation hydrates cached rows before showing placeholders", () => {
  assert.match(appJs, /function saveDerivedGroupCachesFromLoadedItems/);
  assert.match(appJs, /state\.listTotal === loadedItems\.length/);
  assert.match(appJs, /function seedGroupCacheFromAllCache/);
  assert.match(appJs, /function seedScopedCacheFromCurrentItems/);
  assert.match(appJs, /function showInstantListOrPending/);
  assert.match(appJs, /seedGroupCacheFromAllCache\(params\);\s*seedScopedCacheFromCurrentItems\(params\);\s*if \(hydrateListCache\(params\)\) return true;/);
  assert.match(appJs, /showInstantListOrPending\(getBackendListParams\(\)\);\s*loadItemsInBackground\(\);/);
});

test("search sort and owner filter avoid forced skeleton when cached data exists", () => {
  assert.match(appJs, /showInstantListOrPending\(getBackendListParams\(\), \{\s*total: state\.listTotal \|\| 0,[\s\S]*keepCurrentOnMiss: true,/);
  assert.match(appJs, /state\.search = e\.target\.value;[\s\S]*showInstantListOrPending\(getBackendListParams\(\), \{ keepCurrentOnMiss: true \}\);[\s\S]*loadItemsInBackground\(\);/);
  assert.match(appJs, /showInstantListOrPending\(getBackendListParams\(\), \{ total: 0, totalLabel: '\.\.\.', placeholderRows: 10 \}\);[\s\S]*Promise\.all\(\[loadGroups\(\), loadItems\(\)\]\)/);
  assert.doesNotMatch(appJs, /state\.adminFilterUserId = option\.dataset\.ownerFilterOption[\s\S]{0,400}invalidateItemCaches\(\)/);
});
