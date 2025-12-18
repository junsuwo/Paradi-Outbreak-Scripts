
# Paradi Outbreak – Last of Defense (Scripts)

## 📌 Repository Purpose
본 저장소는 프로젝트 특성상 대형 리소스(모델, 텍스처, 사운드 등)를 제외하고  
**스크립트와 구조 중심으로 설계 의도와 구현 방식을 공개**하기 위해 구성되었습니다.

Unity 기반 3D 협동 디펜스 게임  
최대 4인이 협력하여 Titan 웨이브를 방어하는 멀티플레이 프로젝트입니다.

---

## 🎮 Project Overview
- Engine: Unity
- Genre: 3D Cooperative Defense
- Players: Up to 4
- Network: Photon PUN
- Backend: PHP + MySQL

---

## 🔧 My Role
- UI/UX 시스템 설계 및 구현
- Photon PUN 기반 멀티플레이 UI 동기화
- 인벤토리 & 퀵슬롯 DB 저장 / 로드
- 게임 클리어 결과창 UI 및 전투 데이터 기록

---

## 🗂 Repository Structure
```text
Scripts/
 ├ Core        # 게임 흐름 / 공용 시스템
 ├ UI          # HUD, 로그인, 캐릭터 선택 등 UI 전반
 ├ Inventory   # 아이템 / 인벤토리 로직
 ├ Photon      # 멀티플레이 동기화 / RPC
 ├ DB          # 서버 통신 / 저장-로드
 └ Util        # 공용 유틸리티
```
---

## 🎥 Demo
- Gameplay Video:(https://youtu.be/olwNYIAVD0Y)
- Build File: [(Google Drive / OneDrive 링크)](https://drive.google.com/file/d/1h-FZ6oPTJWWPecvo8y4MONjpvAbZ_zIU/view?usp=drive_link)
