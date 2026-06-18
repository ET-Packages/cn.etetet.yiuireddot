# YIUI RedDot 文档索引

## 先读什么

- 想快速了解这套红点怎么用：
  - 读 `01-红点系统总览.md`
- 想新增红点 key：
  - 先读 `02-红点Key与ID规则.md`
- 想让 AI 直接帮你加红点：
  - 先读 `03-AI新增红点工作流.md`
- 想判断这次需求到底是“只改配置”还是“必须补业务代码”：
  - 读 `04-红点配置边界与落地原则.md`
- 想明确红点系统到底该管什么、不该管什么：
  - 读 `05-红点基础规则与职责边界.md`
- 想知道 UI 上怎么绑定和显示红点：
  - 读 `06-红点UI绑定与预制体规范.md`
- 想知道运行时怎么动态给节点挂红点：
  - 读 `07-动态红点绑定.md`
- 想知道红点为什么“没效果”，以及应该怎么逐层排查：
  - 读 `08-红点排查与验证机制.md`

## 本目录目标

- 这套文档不是讲 YIUI 红点包底层源码细节。
- 这套文档的目标是让后续开发者或 AI 在拿到一句很短的需求时，也能先按统一规则补齐上下文，再决定：
  - 是否新增 key
  - key 放在哪一层
  - key 的 id 应该怎么分配
  - 这次改动只需要 `RedDotKeyAsset` / `RedDotConfigAsset`
  - 还是还要补客户端业务状态、消息触发和消除条件

## 当前约束

- 红点 key 运行时来源不是 `RedDotKeyAsset`，而是 `ET.ERedDotKeyType`。
- `RedDotKeyAsset` 负责 key 描述。
- `RedDotConfigAsset` 负责父子依赖关系与 `SwitchTips`。
- 所以“新增红点 key”至少要同时看 3 处：
  - `Scripts/Model/Share/ERedDotKeyType.cs`
  - `Assets/GameRes/RedDot/RedDotKeyAsset.asset`
  - `Assets/GameRes/RedDot/RedDotConfigAsset.asset`

## 后续维护原则

- 不要把所有知识都堆进一个文档。
- 新增规则时，优先补到对应专题文档，再在本 README 增加入口。
