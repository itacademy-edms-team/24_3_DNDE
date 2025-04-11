import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import SkillCard from './SkillCard';

import SkillsSVG from './assets/SkillsFrame.svg'

function SkillList({
  character,
  onSkillProficiencyChange
}) {
  return (
    <Card className="border-0">
      <Card.Img src={SkillsSVG} />
      <Card.ImgOverlay className="d-flex flex-column gap-0 border-0 p-3 pt-1 pb-1">
        <SkillCard
          character={character}
          skill={{
            fieldName: "acrobatics",
            checkControlId: "characterAcrobaticsProficiency",
            resultControlId: "characterAcrobaticsSkillResult",
            labelControlId: "characterAcrobaticsSkillLabel",
            labelText: "Акробатика (Лов)",
            bondAbility: "dexterity"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "investigation",
            checkControlId: "characterInvestigationProficiency",
            resultControlId: "characterInvestigationSkillResult",
            labelControlId: "characterInvestigationSkillLabel",
            labelText: "Анализ (Инт)",
            bondAbility: "intelligence"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "perception",
            checkControlId: "characterPerceptionProficiency",
            resultControlId: "characterPerceptionSkillResult",
            labelControlId: "characterPerceptionSkillLabel",
            labelText: "Восприятие (Муд)",
            bondAbility: "wisdom"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "survival",
            checkControlId: "characterSurvivalProficiency",
            resultControlId: "characterSurvivalSkillResult",
            labelControlId: "characterSurvivalSkillLabel",
            labelText: "Выживание (Муд)",
            bondAbility: "wisdom"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "performance",
            checkControlId: "characterPerformanceProficiency",
            resultControlId: "characterPerformanceSkillResult",
            labelControlId: "characterPerformanceSkillLabel",
            labelText: "Выступление (Хар)",
            bondAbility: "charisma"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "intimidation",
            checkControlId: "characterIntimidationProficiency",
            resultControlId: "characterIntimidationSkillResult",
            labelControlId: "characterIntimidationSkillLabel",
            labelText: "Запугивание (Хар)",
            bondAbility: "charisma"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "survival", // Если это повторяющийся навык – уточните, либо измените fieldName
            checkControlId: "characterSurvivalProficiency",
            resultControlId: "characterSurvivalSkillResult",
            labelControlId: "characterSurvivalSkillLabel",
            labelText: "Выживание (Муд)",
            bondAbility: "wisdom"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "history",
            checkControlId: "characterHistoryProficiency",
            resultControlId: "characterHistorySkillResult",
            labelControlId: "characterHistorySkillLabel",
            labelText: "История (Инт)",
            bondAbility: "intelligence"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "sleightOfHand",
            checkControlId: "characterSleightOfHandProficiency",
            resultControlId: "characterSleightOfHandSkillResult",
            labelControlId: "characterSleightOfHandSkillLabel",
            labelText: "Ловкость рук (Лов)",
            bondAbility: "dexterity"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "arcana",
            checkControlId: "characterArcanaProficiency",
            resultControlId: "characterArcanaSkillResult",
            labelControlId: "characterArcanaSkillLabel",
            labelText: "Магия (Инт)",
            bondAbility: "intelligence"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "medicine",
            checkControlId: "characterMedicineProficiency",
            resultControlId: "characterMedicineSkillResult",
            labelControlId: "characterMedicineSkillLabel",
            labelText: "Медицина (Муд)",
            bondAbility: "wisdom"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "deception",
            checkControlId: "characterDeceptionProficiency",
            resultControlId: "characterDeceptionSkillResult",
            labelControlId: "characterDeceptionSkillLabel",
            labelText: "Обман (Хар)",
            bondAbility: "charisma"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "nature",
            checkControlId: "characterNatureProficiency",
            resultControlId: "characterNatureSkillResult",
            labelControlId: "characterNatureSkillLabel",
            labelText: "Природа (Инт)",
            bondAbility: "intelligence"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "insight",
            checkControlId: "characterInsightProficiency",
            resultControlId: "characterInsightSkillResult",
            labelControlId: "characterInsightSkillLabel",
            labelText: "Проницательность (Муд)",
            bondAbility: "wisdom"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "religion",
            checkControlId: "characterReligionProficiency",
            resultControlId: "characterReligionSkillResult",
            labelControlId: "characterReligionSkillLabel",
            labelText: "Религия (Инт)",
            bondAbility: "intelligence"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "stealth",
            checkControlId: "characterStealthProficiency",
            resultControlId: "characterStealthSkillResult",
            labelControlId: "characterStealthSkillLabel",
            labelText: "Скрытность (Лов)",
            bondAbility: "dexterity"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "persuasion",
            checkControlId: "characterPersuasionProficiency",
            resultControlId: "characterPersuasionSkillResult",
            labelControlId: "characterPersuasionSkillLabel",
            labelText: "Убеждение (Хар)",
            bondAbility: "charisma"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />
        <SkillCard
          character={character}
          skill={{
            fieldName: "animalHandling",
            checkControlId: "characterAnimalHandlingProficiency",
            resultControlId: "characterAnimalHandlingSkillResult",
            labelControlId: "characterAnimalHandlingSkillLabel",
            labelText: "Уход за животными (Муд)",
            bondAbility: "wisdom"
          }}
          onSkillProficiencyChange={onSkillProficiencyChange}
        />

        <span className="border-0 text-center text-uppercase fw-bold">
          Навыки
        </span>
      </Card.ImgOverlay>
    </Card>
  );
}

export default SkillList;