# RG_HSceneCrowdReaction

This mod is to change the reaction of the surrounding characters who see a H scene in front of them.

## Features
- Allow the Character to have different reaction when seeing a H scene instead of just staying awkward.
- Allow the characters in pair to start H-animation even they are not involved in H scene. The position, speed of H-animation is randomly selected and will update periodically.
- Allow player to change the clothing or accessory of the H reaction characters.
- Allow player to relocate the H reaction group.
- Allow player to select the animatin of the H reaction group
- Add hotkeys to allow player to control the status of H reaction male characters. (Shift+3 to Shift+8, following the setup of the game)

## How to trigger H-animation:
Before starting a H scene, get the other characters in pair and make sure they have already had sex with each other (Configurable).

---
Excluded H position list
<table>
  <tr>
    <th>Position</th>
    <th>Exclude Reason</th>
  </tr>
  <tr>
    <td>
      キス, 添い寝キス, キスオナニー, 無理やりキス, <br>
      立ちキス, 立ちキス愛撫, 鏡前抱きつきキス立位, <br>
      ソファーキス騎乗位, 胸もみ＋キス＋手マン
    </td>
    <td>Have not figured out the logic of kiss motion</td>
  </tr>
  <tr>
    <td>
      脱力手コキ, 脱力フェラ
    </td>
    <td>Faintness motion</td>
  </tr>
  <tr>
    <td>
      しゃがみオナニー, ディルドオナニー, <br>
      寝オナニー, 角オナニー, シャワーオナニー, <br>
      椅子オナニー, 風呂オナニー, 机椅子オナニー, <br>
      階段オナニー
    </td>
    <td>Single Masturbation</td>
  </tr>
    <tr>
    <td>
      机スパンキング, 椅子スパンキング, <br>
      木馬鞭, 鞭
    </td>
  <td>motion require player control</td>
  </tr>
  <tr>
    <td>
      拘束玩具攻め, 拘束機械姦, 触手A, 触手B
    </td>
    <td>Single motion</td>
  </tr>
    <tr>
    <td>
      W手コキ＋正常位, 胸もみ＋クンニ＋手コキ<br>
      手コキ＋フェラ＋後背位, 壁クンニ＋壁手マンA<br>
      壁クンニ＋壁手マンB, W壁バックA, W壁バックB<br>
      2ペアフェラA, 2ペアフェラB<br>
      2ペア机椅子挿入A, 2ペア机椅子挿入B
    </td>
    <td>4P motion, require logic update</td>
  </tr>
</table>

---
Possible Reaction Type List

<table>
  <tr>
    <th>Condition</th>
    <th>Possible Types</th>
  </tr>
  <tr>
    <td rowspan="2">Serious(まじめ)</td>
    <td>Awkward</td>
  </tr>
  <tr>
    <td>Angry</td>
  </tr>
  <tr>
    <td rowspan="2">Playful(あそび)</td>
    <td>Hurray</td>
  </tr>
  <tr>
    <td>Excited</td>
  </tr>
  <tr>
    <td rowspan="2">Unique(ユニーク)</td>
    <td>Worry</td>
  </tr>
  <tr>
    <td>Happy</td>
  </tr>
  <tr>
    <td>Female's Libido value higher than 50</td>
    <td>Masturbation</td>
  </tr>
  <tr>
    <td>Partner(恋人) involved in H Scene</td>
    <td>Cry</td>
  </tr>
</table>

## Installation
1. Download the plugin from [Releases](https://github.com/hawkeye-e/RG_HSceneCrowdReaction/releases)
2. Extract the zip file and copy to the "BepInEx\plugins" folder in your RG installation directory

## Note:
If you have not installed the Room Gril Paradise expansion, please stay at the version 0.5.3.
