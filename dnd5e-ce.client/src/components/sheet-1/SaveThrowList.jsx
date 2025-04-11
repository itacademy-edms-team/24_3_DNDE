import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import SaveThrowCard from './SaveThrowCard';

import SavethrowsSVG from './assets/Savethrows.svg';

function SaveThrowList({
  character,
  onSaveThrowProficiencyChange
}) {
  return (
    <Card className="border-0">
      <Card.Img src={SavethrowsSVG} />
      <Card.ImgOverlay className="d-flex flex-column gap-0 border-0 p-3 pt-1 pb-1">
        <SaveThrowCard
          character={character}
          savethrow={{
            checkControlId: "characterStrengthSavethrowProficiencyCheck",
            resultControlId: "characterStrengthSavethrowProficiencyResult",
            labelControlId: "characterStrengthSavethrowProficiencyLabel",
            labelText: "Сила",
            bondAbility: "strength"
          }}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
        />
        <SaveThrowCard
          character={character}
          savethrow={{
            checkControlId: "characterDexteritySavethrowProficiencyCheck",
            resultControlId: "characterDexteritySavethrowProficiencyResult",
            labelControlId: "characterDexteritySavethrowProficiencyLabel",
            labelText: "Ловкость",
            bondAbility: "dexterity"
          }}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
        />
        <SaveThrowCard
          character={character}
          savethrow={{
            checkControlId: "characterConstitutionSavethrowProficiencyCheck",
            resultControlId: "characterConstitutionSavethrowProficiencyResult",
            labelControlId: "characterConstitutionSavethrowProficiencyLabel",
            labelText: "Телосложение",
            bondAbility: "constitution"
          }}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
        />
        <SaveThrowCard
          character={character}
          savethrow={{
            checkControlId: "characterIntelligenceSavethrowProficiencyCheck",
            resultControlId: "characterIntelligenceSavethrowProficiencyResult",
            labelControlId: "characterIntelligenceSavethrowProficiencyLabel",
            labelText: "Интеллект",
            bondAbility: "intelligence"
          }}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
        />
        <SaveThrowCard
          character={character}
          savethrow={{
            checkControlId: "characterWisdomSavethrowProficiencyCheck",
            resultControlId: "characterWisdomSavethrowProficiencyResult",
            labelControlId: "characterWisdomSavethrowProficiencyLabel",
            labelText: "Мудрость",
            bondAbility: "wisdom"
          }}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
        />
        <SaveThrowCard
          character={character}
          savethrow={{
            checkControlId: "characterCharismaSavethrowProficiencyCheck",
            resultControlId: "characterCharismaSavethrowProficiencyResult",
            labelControlId: "characterCharismaSavethrowProficiencyLabel",
            labelText: "Харизма",
            bondAbility: "charisma"
          }}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
        />

        <span className="border-0 text-center text-uppercase fw-bold">
          Спасброски
        </span>
      </Card.ImgOverlay>
    </Card>
  );
}

export default SaveThrowList;