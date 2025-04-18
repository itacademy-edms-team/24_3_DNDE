import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';

import InspirationCard from './InspirationCard';
import ProficiencyBonusCard from './ProficiencyBonusCard';
import SaveThrowList from './SaveThrowList';
import CharacteristicList from './CharacteristicList';
import SkillList from './SkillList';
import PassivePerceptionCard from './PassivePerceptionCard';
import ToolProficienciesAndCustomSkillsCard from './ToolProficienciesAndCustomSkillsCard';
import OtherProficienciesAndCustomSkillsCard from './OtherProficienciesAndCustomSkillsCard';
import AttacksCard from './AttacksCard';
import ArmorClassCard from './ArmorClassCard';
import CharacterInitiativeCard from './CharacterInitiativeCard';
import CharacterSpeedCard from './CharacterSpeedCard';
import CurrentHitsCard from './CurrentHitsCard';
import TempHitsCard from './TempHitsCard';
import HitDiceCard from './HitDiceCard';
import DeathSaveThrowsCard from './DeathSaveThrowsCard';
import PersonalityTraitsCard from './PersonalityTraitsCard';
import CharacterIdeals from './CharacterIdeals';
import CharacterBonds from './CharacterBonds';
import CharacterFlaws from './CharacterFlaws';
import ClassResource from './ClassResource';


function Sheet1Body({
  character,
  onCharacteristicChange,
  onInspirationChange,
  onSaveThrowProficiencyChange,
  onSkillProficiencyChange,
  onToolsChange,
  onToolChange,
  onOtherToolsChange,
  onOtherToolChange,
  onAttacksChange,
  onAttackChange,
  onArmorClassChange,
  onCharacterInitiativeChange,
  onCharacterSpeedChange,
  onCharacterMaxHPChange,
  onCharacterCurrentHPChange,
  onCharacterTempHPChange,
  onHitDiceTotalChange,
  onHitDiceCurrentChange,
  onHitDiceTypeChange,
  onDeathSaveThrowSuccessesChange,
  onDeathSaveThrowFailuresChange,
  onCharacterPersonalityTraitsChange,
  onCharacterIdealsChange,
  onCharacterBondsChange,
  onCharacterFlawsChange,
  onCharacterClassResourceTotalChange,
  onCharacterClassResourceCurrentChange,
  onCharacterClassResourceNameChange,
}) {

  return (
    <Container className='mt-4'>
      <Row>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <Row>
            <Col md={4} className="d-flex flex-column gap-3">
              <CharacteristicList
                character={character}
                onCharacteristicChange={onCharacteristicChange}
              />
            </Col>
            <Col md={8} className="d-flex flex-column gap-3">
              <InspirationCard
                character={character}
                onInspirationChange={onInspirationChange}
              />
              <ProficiencyBonusCard
                character={character}
              />
              <SaveThrowList
                character={character}
                onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
              />
              <SkillList
                character={character}
                onSkillProficiencyChange={onSkillProficiencyChange}
              />
            </Col>
          </Row>
          <Row className="d-flex flex-column gap-3">
            <PassivePerceptionCard
              character={character}
            />
            <ToolProficienciesAndCustomSkillsCard
              character={character}
              onToolsChange={onToolsChange}
              onToolChange={onToolChange}
            />
            <OtherProficienciesAndCustomSkillsCard
              character={character}
              onToolsChange={onOtherToolsChange}
              onToolChange={onOtherToolChange}
            />
          </Row>
        </Col>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <Row>
            <Col md={4}>
              <ArmorClassCard
                character={character}
                onArmorClassChange={onArmorClassChange}
              />
            </Col>
            <Col md={4}>
              <CharacterInitiativeCard
                character={character}
                onCharacterInitiativeChange={onCharacterInitiativeChange}
              />
            </Col>
            <Col md={4}>
              <CharacterSpeedCard
                character={character}
                onCharacterSpeedChange={onCharacterSpeedChange}
              />
            </Col>
          </Row>
          <Row>
            <Col className="d-flex flex-column gap-3">
              <CurrentHitsCard
                character={character}
                onCharacterMaxHPChange={onCharacterMaxHPChange}
                onCharacterCurrentHPChange={onCharacterCurrentHPChange}
              />
              <TempHitsCard
                character={character}
                onCharacterTempHPChange={onCharacterTempHPChange}
              />
              <Row>
                <Col md={6}>
                  <HitDiceCard
                    character={character}
                    onHitDiceTotalChange={onHitDiceTotalChange}
                    onHitDiceCurrentChange={onHitDiceCurrentChange}
                    onHitDiceTypeChange={onHitDiceTypeChange}
                  />
                </Col>
                <Col md={6}>
                  <DeathSaveThrowsCard
                    character={character}
                    onDeathSaveThrowSuccessesChange={onDeathSaveThrowSuccessesChange}
                    onDeathSaveThrowFailuresChange={onDeathSaveThrowFailuresChange}
                  />
                </Col>
              </Row>
            </Col>
          </Row>
          <Row>
            <AttacksCard
              character={character}
              onAttacksChange={onAttacksChange}
              onAttackChange={onAttackChange}
            />
          </Row>
        </Col>
        <Col md={6} lg={4} className="d-flex flex-column gap-3">
          <Row>
            <Col className="d-flex flex-column gap-3">
              <PersonalityTraitsCard
                character={character}
                onCharacterPersonalityTraitsChange={onCharacterPersonalityTraitsChange}
              />
              <CharacterIdeals
                character={character}
                onCharacterIdealsChange={onCharacterIdealsChange}
              />
              <CharacterBonds
                character={character}
                onCharacterBondsChange={onCharacterBondsChange}
              />
              <CharacterFlaws
                character={character}
                onCharacterFlawsChange={onCharacterFlawsChange}
              />
            </Col>
          </Row>
          <Row>
            <Col>
              <Button
                variant="primary"
                className="h-100"
              >
                Короткий отдых
              </Button>
            </Col>
            <Col>
              <Button
                variant="primary"
                className="h-100"
              >
                Длительный отдых
              </Button>
            </Col>
          </Row>
          <Row>
            {/*<CharacterResourceContainer*/}
            {/*  character={character}*/}
            {/*/>*/}
          </Row>
        </Col>
      </Row>
    </Container>
  );
}

export default Sheet1Body;