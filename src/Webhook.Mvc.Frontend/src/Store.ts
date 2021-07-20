import { action, observable, runInAction } from 'mobx';
import { RequestRecordDetailPayload } from './api/IWebhookCoreHub';
import { createContext, useContext } from 'react';

export interface WebhookInViewInspectorConfig {
  Position?: 'Bottom' | 'Top';
  PathBase: string;
  RequestId: string;
}

export class SubRequestFailurePayload {
  Id: string = new Date().valueOf().toString() + '.' + Math.random();
  constructor(public Path: string, public ResponseStatusCode: number) {}
}

export type SubRequestPayload = RequestRecordDetailPayload | SubRequestFailurePayload;

export class WebhookInViewInspectorStore {
  @observable
  data?: RequestRecordDetailPayload;

  @observable
  position: 'Bottom' | 'Top' = 'Top';

  @observable
  subRequests: SubRequestPayload[] = [];

  @observable
  config!: WebhookInViewInspectorConfig;

  @observable
  isReady = false;

  private apiEndPointBase!: string;

  @action.bound
  async ready(config: WebhookInViewInspectorConfig) {
    this.config = config;
    this.apiEndPointBase = getApiEndPointBase(config);
    this.position = config.Position || 'Bottom';

    const detail = await this.getDetailByIdAsync(config.RequestId);

    runInAction(() => {
      this.data = detail;
      this.isReady = true;
    });
  }

  @action.bound
  async fetchSubRequestById(id: string) {
    const detail = await this.getDetailByIdAsync(id);

    runInAction(() => {
      this.subRequests = this.subRequests.concat([detail]);
    });
  }

  @action.bound
  addFailureSubRequest(url: string, status: number) {
    this.subRequests = this.subRequests.concat([new SubRequestFailurePayload(url, status)]);
  }

  private async getDetailByIdAsync(id: string): Promise<RequestRecordDetailPayload> {
    return fetch(`${this.apiEndPointBase}/GetDetailById?id=${id}`).then((x) => x.json());
  }
}

function getApiEndPointBase(config: WebhookInViewInspectorConfig) {
  const isDevelopment = process.env.NODE_ENV === 'development';
  const host = isDevelopment
    ? location.search.match(/__rin__dev__host=([^&]+)/)
      ? location.search.match(/__rin__dev__host=([^&]+)/)![1]
      : 'localhost:5000'
    : location.host;
  // const protocol = location.protocol === 'http:' ? 'ws:' : 'wss:';
  // const pathBase = isDevelopment ? '/' : config.PathBase || '/rin';
  const endPointBasePath = config.PathBase || '/rin';
  // const channelEndPoint = `${protocol}//${host}${endPointBasePath}/chan`;
  const urlBase = `${location.protocol}//${host}${endPointBasePath}/api`;

  return urlBase;
}

export const rinInViewInspectorStore = new WebhookInViewInspectorStore();
const rinInViewInspectorStoreContext = createContext(rinInViewInspectorStore);
export const useRinInViewInspectorStore = () => useContext(rinInViewInspectorStoreContext);
