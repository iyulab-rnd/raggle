import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('chatting-page')
export class ChattingPage extends LitElement {

  render() {
    return html`
      <main-layout ratio="1:2:2">
        <main-list slot="left"
          
        ></main-list>
        <main-chat slot="main">
          
        </main-chat>
        <div slot="right">
          
        </div>
      </main-layout>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
  `;
}